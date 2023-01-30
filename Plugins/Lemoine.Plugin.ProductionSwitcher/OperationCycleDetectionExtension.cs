// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ProductionSwitcher
{
  /// <summary>
  /// Description of OperationCycleDetectionExtension.
  /// </summary>
  public class OperationCycleDetectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Analysis.IOperationCycleDetectionExtension
    , IObservationStateSlotChangeListener
  {
    #region Members
    IMonitoredMachine m_machine = null;
    bool m_initializedConfiguration = false;
    bool m_active = false;
    IMachineStateTemplate m_productionMachineStateTemplate = null;
    IEnumerable<IMachineStateTemplate> m_setupMachineStateTemplates = null;
    double m_cycleDurationMargin = 1.0;
    double m_betweenCyclesDurationMargin = 1.0;
    
    bool m_pendingChanges = false;
    object m_observationStateSlotLock = new object ();
    volatile bool m_observationStateSlotLoaded = false; // was observationstateslot loaded for the current detection process ?
    IObservationStateSlot m_observationStateSlot = null;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (OperationCycleDetectionExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public OperationCycleDetectionExtension ()
      : base (new ConfigurationLoader ())
    {
    }
    #endregion // Constructors

    #region IObservationStateSlotChangeListener implementation
    public void NotifyObservationStateSlotChange(IObservationStateSlot slot)
    {
      Debug.Assert (null != slot);
      
      if (!m_observationStateSlotLoaded) { // Not loaded => nothing to do
        return;
      }
      
      if (object.Equals (slot.Machine, m_machine)) {
        lock (m_observationStateSlotLock)
        {
          if ( (null != m_observationStateSlot)
              && slot.DateTimeRange.Overlaps (m_observationStateSlot.DateTimeRange)) {
            // m_observationStateSlot is not valid any more
            m_observationStateSlot = null;
            m_observationStateSlotLoaded = false;
          }
        }
      }
    }
    #endregion // IObservationStateSlotChangeListener implementation


    #region IOperationCycleDetectionExtension implementation
    public bool Initialize (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);
      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                 typeof (OperationCycleDetectionExtension).FullName,
                                                 machine.Id));
      ObservationStateSlotChangeNotifier.AddListener (this);

      return true;
    }
    
    /// <summary>
    /// Start a detection process for the specified machine module
    /// </summary>
    public void DetectionProcessStart ()
    {
      m_observationStateSlotLoaded = false;
    }
    
    /// <summary>
    /// Complete a detection process for the specified machine module
    /// </summary>
    public void DetectionProcessComplete ()
    { }
    
    /// <summary>
    /// An error was raised during the detection process
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="ex"></param>
    public void DetectionProcessError (IMachineModule machineModule, Exception ex)
    {
      // There might have been a rollback:
      // some cache value are not valid any more
      m_pendingChanges = false;
    }

    public void StartCycle(IOperationCycle operationCycle)
    {
      // Nothing to do here
      return;
    }
    
    public void StopCycle(IOperationCycle operationCycle)
    {
      Debug.Assert (null != m_machine);
      
      if (!operationCycle.Full) {
        log.DebugFormat ("StopCycle: " +
                         "operationCycle is not full, skip it");
        return;
      }
      
      InitializeConfiguration ();
      
      Debug.Assert (null != operationCycle);
      Debug.Assert (operationCycle.End.HasValue);
      Debug.Assert (null != operationCycle.Machine);
      Debug.Assert (object.Equals (m_machine, operationCycle.Machine));
      
      DateTime dateTime = operationCycle.End.Value;
      if (!IsActive (dateTime)) {
        log.DebugFormat ("StopCycle: " +
                         "production detection is not active at {0} => return",
                         dateTime);
        return;
      }
      
      if (null == operationCycle.OperationSlot) {
        log.WarnFormat ("StopCycle: " +
                        "operation cycle {0} is not associated to any operation slot",
                        operationCycle);
        return;
      }
      
      if (null == operationCycle.OperationSlot.Operation) {
        log.WarnFormat ("StopCycle: " +
                        "operation cycle {0} is not associated to any operation",
                        operationCycle);
        return;
      }
      IOperation operation = operationCycle.OperationSlot.Operation;
      
      if ((0.0 < m_betweenCyclesDurationMargin)
          && (0.0 < GetStandardBetweenDuration (operation).TotalSeconds)) {
        log.DebugFormat ("StopCycle: " +
                         "the between duration must be considered, return");
        return;
      }
      
      if (IsGoodCycle (operationCycle)) {
        // There is no need to check the between cycles duration,
        // switch to the new machine state template from operationCycle.Begin
        SwitchToProduction (operationCycle.Begin.Value);
      }
    }
    
    public void CreateBetweenCycle(IBetweenCycles betweenCycles)
    {
      Debug.Assert (null != m_machine);
      
      InitializeConfiguration ();
      
      if (!IsActive (betweenCycles.End)) {
        log.DebugFormat ("CreateBetweenCycle: " +
                         "production detection is not active at {0} => return",
                         betweenCycles.End);
        return;
      }
      
      // Check the previous cycle
      if (!IsGoodCycle (betweenCycles.PreviousCycle)) {
        log.DebugFormat ("CreateBetweenCycle: " +
                         "cycle {0} is not a good cycle",
                         betweenCycles.PreviousCycle);
        return;
      }
      // Because previousCycle is a good cycle:
      Debug.Assert (betweenCycles.PreviousCycle.Begin.HasValue);
      Debug.Assert (null != betweenCycles.PreviousCycle.OperationSlot);
      Debug.Assert (null != betweenCycles.PreviousCycle.OperationSlot.Operation);
      
      if (m_betweenCyclesDurationMargin <= 0.0) {
        log.DebugFormat ("CreateBetweenCycle: " +
                         "no margin for between cycles, and the cycle {0} is a good one " +
                         "=> switch to production from {1}",
                         betweenCycles.PreviousCycle,
                         betweenCycles.PreviousCycle.Begin.Value);
        SwitchToProduction (betweenCycles.PreviousCycle.Begin.Value);
        return;
      }
      
      TimeSpan standardBetweenDuration = GetStandardBetweenDuration (betweenCycles.PreviousCycle.OperationSlot.Operation);
      if (standardBetweenDuration.TotalSeconds <= 0.0) {
        log.DebugFormat ("CreateBetweenCycle: " +
                         "no standard between duration and the cycle {0} is a good one " +
                         "=> switch to production from {1}",
                         betweenCycles.PreviousCycle,
                         betweenCycles.PreviousCycle.Begin.Value);
        SwitchToProduction (betweenCycles.PreviousCycle.Begin.Value);
        return;
      }
      
      TimeSpan betweenDuration = betweenCycles.End.Subtract (betweenCycles.Begin);
      if (betweenDuration.TotalSeconds <= standardBetweenDuration.TotalSeconds * m_betweenCyclesDurationMargin) {
        log.DebugFormat ("CreateBetweenCycle: " +
                         "{0} is a good cycle and the between cycle duration is good " +
                         "=> switch to production from {1}",
                         betweenCycles.PreviousCycle,
                         betweenCycles.PreviousCycle.Begin.Value);
        SwitchToProduction (betweenCycles.PreviousCycle.Begin.Value);
        return;
      }
    }
    #endregion
    
    void InitializeConfiguration ()
    {
      Debug.Assert (null != m_machine);
      
      if (m_initializedConfiguration) { // Already initialized
        return;
      }
      
      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.ErrorFormat ("InitializeConfiguration: " +
                         "the configuration is not valid, skip this instance");
        m_productionMachineStateTemplate = null;
        m_setupMachineStateTemplates = null;
        m_initializedConfiguration = true;
        return;
      }
      
      IMachineFilter machineFilter = null;
      if (0 < configuration.MachineFilterId) { // Machine filter
        // Initialize m_productionMachineStateTemplateId
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.InitializeConfiguration.MachineFilter"))
          {
            int machineFilterId = configuration.MachineFilterId;
            if (0 != machineFilterId) {
              machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
                .FindById (machineFilterId);
              if (null == machineFilter) {
                log.ErrorFormat ("Initialize: " +
                                 "machine filter id {0} does not exist",
                                 machineFilterId);
                m_active = false;
                m_productionMachineStateTemplate = null;
                m_setupMachineStateTemplates = null;
                m_initializedConfiguration = true;
                return;
              }
            }
          }
        }
      }
      if ( (null != machineFilter) && !machineFilter.IsMatch (m_machine)) {
        m_active = false;
      }
      else {
        m_active = true;
      }
      
      { // Get m_productionMachineStateTemplate
        if (0 < configuration.ProductionMachineStateTemplateId) { // Valid integer
          // Initialize m_productionMachineStateTemplateId
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.InitializeConfiguration.1"))
            {
              m_productionMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                .FindById (configuration.ProductionMachineStateTemplateId);
            }
          }
          
          if (null == m_productionMachineStateTemplate) {
            log.ErrorFormat ("Initialize: " +
                             "no machine state template found for ID={0}",
                             configuration.ProductionMachineStateTemplateId);
          }
        }
      }
      
      { // Get m_setupMachineStateTemplates
        var setupMachineStateTemplates = new List<IMachineStateTemplate> ();
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          foreach (var setupMachineStateTemplateId in configuration.SetupMachineStateTemplateIds) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.InitializeConfiguration.2"))
            {
              var setupMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                .FindById (setupMachineStateTemplateId);
              if (null != setupMachineStateTemplate) {
                setupMachineStateTemplates.Add (setupMachineStateTemplate);
              }
              else {
                log.ErrorFormat ("Initialize: " +
                                 "no machine state template found for ID={0}",
                                 setupMachineStateTemplateId);
              }
            }
          }
        }
        m_setupMachineStateTemplates = setupMachineStateTemplates;
      }

      { // m_cycleDurationMargin
        m_cycleDurationMargin = configuration.CycleDurationPercentageTrigger / 100.0;
      }

      { // m_betweenCyclesDurationMargin
        m_betweenCyclesDurationMargin = configuration.BetweenCyclesDurationPercentageTrigger / 100.0;
      }

      m_initializedConfiguration = true;
    }
    
    void InitializeCurrentObservationStateSlot (IMachine machine, DateTime dateTime)
    {
      if (m_observationStateSlotLoaded) {
        // Still in the same detection process:
        // it is useless to try to get another observationstateslot
        // since no new slot was processed since
        return;
      }
      
      lock (m_observationStateSlotLock)
      {
        if (m_pendingChanges
            || (null == m_observationStateSlot)
            || !m_observationStateSlot.DateTimeRange.ContainsElement (dateTime)) {
          IObservationStateSlot previousObservationStateSlot = m_observationStateSlot;
          
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.InitializeCurrentObservationStateSlot"))
            {
              m_observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
                .FindAt (machine, dateTime);
              m_observationStateSlotLoaded = true;
            }
          }
          
          if (m_pendingChanges
              && !object.Equals (previousObservationStateSlot, m_observationStateSlot)) {
            m_pendingChanges = false;
          }
        }
      } // lock
    }

    /// <summary>
    /// Is the production detection active at the given time after checking the current observation state slot
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    bool IsActive (DateTime dateTime)
    {
      if (false == m_active) {
        return false;
      }
      
      lock (m_observationStateSlotLock)
      {
        InitializeCurrentObservationStateSlot (m_machine, dateTime);
        Debug.Assert (null != m_observationStateSlot);
        
        if (m_pendingChanges) {
          log.DebugFormat ("IsActive: " +
                           "there is a pending change, inhibit any process for the moment");
          return false;
        }

        if (object.Equals (m_observationStateSlot.MachineStateTemplate, m_productionMachineStateTemplate)) {
          // Already the production !
          // Nothing to do
          log.DebugFormat ("IsActive: " +
                           "already the production machine state template {0} " +
                           "=> nothing to do, return",
                           m_productionMachineStateTemplate);
          return false;
        }
        
        if ( (null != m_setupMachineStateTemplates)
            && !m_setupMachineStateTemplates.Contains (m_observationStateSlot.MachineStateTemplate)) {
          log.DebugFormat ("IsActive: " +
                           "the current machine state template {0} is not a listed setup state template" +
                           "=> nothing to do, return",
                           m_observationStateSlot.MachineStateTemplate);
          return false;
        }
      } // lock
      
      return true;
    }

    /// <summary>
    /// Check if a specified cycle is a good cycle
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <returns></returns>
    bool IsGoodCycle (IOperationCycle operationCycle)
    {
      IOperation operation = null;
      if (null != operationCycle.OperationSlot) {
        operation = operationCycle.OperationSlot.Operation;
      }
      
      if (null == operation) {
        log.InfoFormat ("IsGoodCycle: " +
                        "no operation is associated to {0} " +
                        "=> return false",
                        operationCycle);
        return false;
      }
      else if (m_cycleDurationMargin <= 0.0) {
        log.InfoFormat ("IsGoodCycle: " +
                        "no cycle duration margin was defined " +
                        "=> the cycle is ok if it is full");
        return operationCycle.Full;
      }
      else if (!operation.MachiningDuration.HasValue) {
        log.InfoFormat ("IsGoodCycle: " +
                        "operation {0} has no machining duration " +
                        "=> the cycle is ok if it is full",
                        operation);
        return operationCycle.Full;
      }
      else if (!operationCycle.Begin.HasValue) {
        log.InfoFormat ("IsGoodCycle: " +
                        "operation cycle {0} has no begin",
                        operationCycle);
        return false;
      }
      else if (!operationCycle.Full) {
        log.InfoFormat ("IsGoodCycle: " +
                        "operation cycle {0} is not a full cycle",
                        operationCycle);
        return false;
      }
      else { // true == operationCycle.Full
        TimeSpan duration = operationCycle.End.Value.Subtract (operationCycle.Begin.Value);
        if (duration.TotalSeconds <= operation.MachiningDuration.Value.TotalSeconds * m_cycleDurationMargin) {
          // This is a good cycle
          log.DebugFormat ("IsGoodCycle: " +
                           "cycle {0} is a good one",
                           operationCycle);
          return true;
        }
        else {
          log.DebugFormat ("IsGoodCycle: " +
                           "cycle {0} is a bad one",
                           operationCycle);
          return false;
        }
      }
    }

    void SwitchToProduction (DateTime from)
    {
      Debug.Assert (null != m_machine);
      
      log.DebugFormat ("SwitchToProduction: " +
                       "from {0}",
                       from);
      
      UtcDateTimeRange range = new UtcDateTimeRange (from);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("ProductionSwitcher.SwitchToProduction"))
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (m_machine,
                                                    m_productionMachineStateTemplate,
                                                    range);
          association.Option = AssociationOption.Detected;
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
          transaction.Commit ();
        }
      }
      m_pendingChanges = true;
    }

    TimeSpan GetStandardBetweenDuration (IOperation operation)
    {
      if (m_machine.PalletChangingDuration.HasValue) {
        log.DebugFormat ("GetStandardBetweenDuration: " +
                         "from pallet changing duration: {0}",
                         m_machine.PalletChangingDuration.Value);
        return m_machine.PalletChangingDuration.Value;
      }
      
      TimeSpan duration = TimeSpan.FromSeconds (0);
      if (operation.LoadingDuration.HasValue) {
        duration = duration.Add (operation.LoadingDuration.Value);
      }
      if (operation.UnloadingDuration.HasValue) {
        duration = duration.Add (operation.UnloadingDuration.Value);
      }
      log.DebugFormat ("GetStandardBetweenDuration: " +
                       "duration is {0}",
                       duration);
      return duration;
    }
  }
}
