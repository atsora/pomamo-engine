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

    ILog log = LogManager.GetLogger (typeof (OperationCycleDetectionExtension).FullName);

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
      log = LogManager.GetLogger ($"{typeof (OperationCycleDetectionExtension).FullName}.{machine.Id}");
      // Note: the listener is never removed, see the comment in ObservationStateSlotChangeNotifier
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
      
      Debug.Assert (null != operationCycle);
      Debug.Assert (null != operationCycle.Machine);
      Debug.Assert (object.Equals (m_machine, operationCycle.Machine));

      if (!operationCycle.Full) {
        if (log.IsDebugEnabled) {
          log.Debug ("StopCycle: operationCycle is not full, skip it");
        }
        return;
      }

      if (!operationCycle.End.HasValue) {
        log.Error ($"StopCycle: operation cycle {operationCycle} is full but has no end => return");
        return;
      }

      InitializeConfiguration ();

      DateTime dateTime = operationCycle.End.Value;
      if (!IsActive (dateTime)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"StopCycle: production detection is not active at {dateTime} => return");
        }
        return;
      }

      if (null == operationCycle.OperationSlot) {
        if (log.IsWarnEnabled) {
          log.Warn ($"StopCycle: operation cycle {operationCycle} is not associated to any operation slot");
        }
        return;
      }

      if (null == operationCycle.OperationSlot.Operation) {
        if (log.IsWarnEnabled) {
          log.Warn ($"StopCycle: operation cycle {operationCycle} is not associated to any operation");
        }
        return;
      }
      IOperation operation = operationCycle.OperationSlot.Operation;

      if ((0.0 < m_betweenCyclesDurationMargin)
          && (0.0 < GetStandardBetweenDuration (operation).TotalSeconds)) {
        if (log.IsDebugEnabled) {
          log.Debug ("StopCycle: the between duration must be considered, return");
        }
        return;
      }

      if (IsGoodCycle (operationCycle)) {
        // There is no need to check the between cycles duration,
        // switch to the new machine state template from operationCycle.Begin
        Debug.Assert (operationCycle.Begin.HasValue); // Guaranteed by IsGoodCycle
        SwitchToProduction (operationCycle.Begin.Value);
      }
    }
    
    public void CreateBetweenCycle(IBetweenCycles betweenCycles)
    {
      Debug.Assert (null != m_machine);
      Debug.Assert (null != betweenCycles);

      InitializeConfiguration ();

      if (!IsActive (betweenCycles.End)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateBetweenCycle: production detection is not active at {betweenCycles.End} => return");
        }
        return;
      }

      // Check the previous cycle
      var previousCycle = betweenCycles.PreviousCycle;
      if (!IsGoodCycle (previousCycle)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateBetweenCycle: cycle {previousCycle} is not a good cycle");
        }
        return;
      }
      // Because previousCycle is a good cycle:
      Debug.Assert (previousCycle.Begin.HasValue);
      Debug.Assert (null != previousCycle.OperationSlot);
      Debug.Assert (null != previousCycle.OperationSlot.Operation);
      var previousCycleBegin = previousCycle.Begin.Value;

      if (m_betweenCyclesDurationMargin <= 0.0) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateBetweenCycle: no margin for between cycles, and the cycle {previousCycle} is a good one => switch to production from {previousCycleBegin}");
        }
        SwitchToProduction (previousCycleBegin);
        return;
      }

      TimeSpan standardBetweenDuration = GetStandardBetweenDuration (previousCycle.OperationSlot.Operation);
      if (standardBetweenDuration.TotalSeconds <= 0.0) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateBetweenCycle: no standard between duration and the cycle {previousCycle} is a good one => switch to production from {previousCycleBegin}");
        }
        SwitchToProduction (previousCycleBegin);
        return;
      }

      TimeSpan betweenDuration = betweenCycles.End.Subtract (betweenCycles.Begin);
      if (betweenDuration.TotalSeconds <= standardBetweenDuration.TotalSeconds * m_betweenCyclesDurationMargin) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateBetweenCycle: {previousCycle} is a good cycle and the between cycle duration is good => switch to production from {previousCycleBegin}");
        }
        SwitchToProduction (previousCycleBegin);
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
      
      // Note: whatever happens below, the initialization is not attempted a second time
      m_initializedConfiguration = true;
      m_active = false;
      m_productionMachineStateTemplate = null;
      m_setupMachineStateTemplates = null;

      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.Error ("InitializeConfiguration: the configuration is not valid, skip this instance");
        return;
      }

      if (0 < configuration.MachineFilterId) { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.InitializeConfiguration.MachineFilter"))
          {
            int machineFilterId = configuration.MachineFilterId;
            var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.Error ($"InitializeConfiguration: machine filter id {machineFilterId} does not exist => skip this instance");
              return;
            }
            // Note: machineFilter.IsMatch requires it is done in the same session
            if (!machineFilter.IsMatch (m_machine)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"InitializeConfiguration: machine {m_machine.Id} does not match the machine filter {machineFilterId} => skip this instance");
              }
              return;
            }
          }
        }
      }

      { // Get m_productionMachineStateTemplate
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.InitializeConfiguration.1"))
          {
            m_productionMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
              .FindById (configuration.ProductionMachineStateTemplateId);
          }
        }

        if (null == m_productionMachineStateTemplate) {
          // Else SwitchToProduction would try to apply a null machine state template
          log.Error ($"InitializeConfiguration: no machine state template found for id {configuration.ProductionMachineStateTemplateId} => skip this instance");
          return;
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
                log.Error ($"InitializeConfiguration: no machine state template found for id {setupMachineStateTemplateId}");
              }
            }
          }
        }
        m_setupMachineStateTemplates = setupMachineStateTemplates;
      }

      m_cycleDurationMargin = configuration.CycleDurationPercentageTrigger / 100.0;
      m_betweenCyclesDurationMargin = configuration.BetweenCyclesDurationPercentageTrigger / 100.0;

      m_active = true;
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

        if (null == m_observationStateSlot) {
          log.Error ($"IsActive: no observation state slot at {dateTime} => return false");
          return false;
        }

        if (m_pendingChanges) {
          if (log.IsDebugEnabled) {
            log.Debug ("IsActive: there is a pending change, inhibit any process for the moment");
          }
          return false;
        }

        if (object.Equals (m_observationStateSlot.MachineStateTemplate, m_productionMachineStateTemplate)) {
          // Already the production !
          // Nothing to do
          if (log.IsDebugEnabled) {
            log.Debug ($"IsActive: already the production machine state template {m_productionMachineStateTemplate} => nothing to do, return");
          }
          return false;
        }

        // Note: an empty list of set-up machine state templates means they all apply
        if ((null != m_setupMachineStateTemplates)
            && m_setupMachineStateTemplates.Any ()
            && !m_setupMachineStateTemplates.Contains (m_observationStateSlot.MachineStateTemplate)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsActive: the current machine state template {m_observationStateSlot.MachineStateTemplate} is not a listed setup state template => nothing to do, return");
          }
          return false;
        }
      } // lock

      return true;
    }

    /// <summary>
    /// Check if a specified cycle is a good cycle
    ///
    /// Note: a good cycle always has a begin and an end, because the caller switches
    /// to the production machine state template from the begin of the cycle
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <returns></returns>
    bool IsGoodCycle (IOperationCycle operationCycle)
    {
      Debug.Assert (null != operationCycle);

      IOperation operation = null;
      if (null != operationCycle.OperationSlot) {
        operation = operationCycle.OperationSlot.Operation;
      }

      if (null == operation) {
        if (log.IsInfoEnabled) {
          log.Info ($"IsGoodCycle: no operation is associated to {operationCycle} => return false");
        }
        return false;
      }

      if (!operationCycle.Full) {
        if (log.IsInfoEnabled) {
          log.Info ($"IsGoodCycle: operation cycle {operationCycle} is not a full cycle => return false");
        }
        return false;
      }

      // Note: a cycle may be flagged as full while it has no begin or no end,
      //       and the begin is required by the caller
      if (!operationCycle.Begin.HasValue) {
        if (log.IsInfoEnabled) {
          log.Info ($"IsGoodCycle: operation cycle {operationCycle} has no begin => return false");
        }
        return false;
      }

      if (!operationCycle.End.HasValue) {
        if (log.IsInfoEnabled) {
          log.Info ($"IsGoodCycle: operation cycle {operationCycle} has no end => return false");
        }
        return false;
      }

      if (m_cycleDurationMargin <= 0.0) {
        if (log.IsInfoEnabled) {
          log.Info ("IsGoodCycle: no cycle duration margin was defined => the full cycle is ok");
        }
        return true;
      }

      if (!operation.MachiningDuration.HasValue) {
        if (log.IsInfoEnabled) {
          log.Info ($"IsGoodCycle: operation {operation} has no machining duration => the full cycle is ok");
        }
        return true;
      }

      TimeSpan duration = operationCycle.End.Value.Subtract (operationCycle.Begin.Value);
      if (duration.TotalSeconds <= operation.MachiningDuration.Value.TotalSeconds * m_cycleDurationMargin) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsGoodCycle: cycle {operationCycle} is a good one");
        }
        return true;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsGoodCycle: cycle {operationCycle} is a bad one");
        }
        return false;
      }
    }

    void SwitchToProduction (DateTime from)
    {
      Debug.Assert (null != m_machine);
      Debug.Assert (null != m_productionMachineStateTemplate);

      if (log.IsDebugEnabled) {
        log.Debug ($"SwitchToProduction: from {from}");
      }

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
        if (log.IsDebugEnabled) {
          log.Debug ($"GetStandardBetweenDuration: from pallet changing duration: {m_machine.PalletChangingDuration.Value}");
        }
        return m_machine.PalletChangingDuration.Value;
      }

      TimeSpan duration = TimeSpan.FromSeconds (0);
      if (operation.LoadingDuration.HasValue) {
        duration = duration.Add (operation.LoadingDuration.Value);
      }
      if (operation.UnloadingDuration.HasValue) {
        duration = duration.Add (operation.UnloadingDuration.Value);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetStandardBetweenDuration: duration is {duration}");
      }
      return duration;
    }
  }
}
