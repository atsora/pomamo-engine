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
using System.Threading;

namespace Lemoine.Plugin.DefaultReasonEndObservationStateSlot
{
  /// <summary>
  /// Track all changes in the observed and planned states of a monitored machine.
  /// Entry points in MonitoredMachineActivityAnalysis, Lemoine.Analysis
  /// 
  /// TODO: track the changes in ObservationStateSlot.MachineObservationState that may influence the default reason
  /// </summary>
  public class ActivityAnalysisExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Analysis.IActivityAnalysisExtension
    , IUpdateDefaultReasonListener
  {
    ILog log = LogManager.GetLogger(typeof (ActivityAnalysisExtension).FullName);
    
    IEnumerable<Configuration> m_configurations = new List<Configuration> ();
    
    IMonitoredMachine m_machine;
    
    // Cache on the observation state slots
    IObservationStateSlot m_observationStateSlot = null;
    bool m_isCurrentObservationStateSlot = false;
    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public IMachine Machine {
      get { return m_machine; }
    }
    #endregion // Getters / Setters
    
    #region IActivityAnalysisExtension implementation
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machine"></param>
    public bool Initialize (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);
      
      log = LogManager.GetLogger(typeof (ActivityAnalysisExtension).FullName + "." + machine.Id);

      m_configurations = LoadConfigurations ();
      
      m_machine = machine;

      UpdateDefaultReasonNotifier.AddListener (this);

      return true;
    }

    /// <summary>
    /// Before processing the activities
    /// </summary>
    /// <param name="lastActivityAnalysisDateTime"></param>
    /// <param name="facts"></param>
    public void BeforeProcessingActivities (DateTime lastActivityAnalysisDateTime, IList<IFact> facts)
    {
    }

    /// <summary>
    /// Before procesing a new activity period
    /// </summary>
    /// <param name="machineStatus"></param>
    public void BeforeProcessingNewActivityPeriod(IMachineStatus machineStatus)
    {
      return;
    }
    
    /// <summary>
    /// Process a new activity period (which is not empty)
    /// </summary>
    /// <param name="activityRange"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="shift"></param>
    public void AfterProcessingNewActivityPeriod(UtcDateTimeRange activityRange,
                                                 IMachineMode machineMode,
                                                 IMachineStateTemplate machineStateTemplate,
                                                 IMachineObservationState machineObservationState,
                                                 IShift shift)
    {
      Debug.Assert (null != m_machine);
      
      if (activityRange.IsEmpty ()) { // Nothing to do
        return;
      }
      
      m_isCurrentObservationStateSlot = m_observationStateSlot.DateTimeRange.ContainsRange (activityRange);
    }

    /// <summary>
    /// Run before the commit of the Activities transaction
    /// </summary>
    public void BeforeActivitiesCommit ()
    {
    }
    
    /// <summary>
    /// Run after a rollback of the Activities transaction
    /// </summary>
    public void AfterActivitiesRollback ()
    {
    }
    #endregion // IActivityAnalysisExtension implementation

    #region IUpdateDefaultReasonListener implementation
    public bool UpdateDefaultReason(CancellationToken cancellationToken, IReasonSlot slot)
    {
      Debug.Assert (null != slot);
      
      // Get the configurations that match
      var validConfigurations = m_configurations
        .Where (c => c.CurrentMachineObservationStateId.Equals (slot.MachineObservationState.Id))
        .Where (c => c.MachineModeIds.Any (id => IsMachineModeIdValid (id, slot.MachineMode)));
      if (!validConfigurations.Any ()) {
        return false;
      }
      
      using (var temporaryCache = new TemporaryCache (this))
      {
        Load (slot.DateTimeRange);
        
        if (null == m_observationStateSlot) { // the machine state template is not computed yet, postpone it
          return false;
        }
        
        if (!Bound.Equals<DateTime> (slot.DateTimeRange.Upper, m_observationStateSlot.DateTimeRange.Upper)) { // End does not match
          return false;
        }
        Debug.Assert (m_observationStateSlot.DateTimeRange.Upper.HasValue); // Because slot.DateTimeRange.Upper.HasValue
        
        bool isNextRequired = validConfigurations
          .Any (c => c.NextMachineObservationStateId.HasValue || c.IsNextSameShift.HasValue);
        IObservationStateSlot next = null;
        if (isNextRequired) {
          var at = m_observationStateSlot.DateTimeRange.Upper.Value;
          next = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAt (m_machine, at);
          Debug.Assert (null != next);
          if (null == next.MachineObservationState) { // Process it
            UtcDateTimeRange applicableRange = new UtcDateTimeRange (at, at.AddSeconds (1));
            bool result = next.ProcessTemplate (cancellationToken, applicableRange, null, false, null, null);
            if (false == result) {
              log.ErrorFormat ("NotifyReasonSlotAddModify: " +
                               "template not processed in observation state slot " +
                               "=> fallback: skip next");
              next = null;
            }
          }
        }
        
        foreach (var configuration in validConfigurations) {
          if (configuration.CurrentMachineObservationStateId != m_observationStateSlot.MachineObservationState.Id) {
            continue;
          }
          if (null != next) {
            if (configuration.IsNextSameShift.HasValue) {
              if (IsSameShift (m_observationStateSlot, next) != configuration.IsNextSameShift.Value) {
                continue;
              }
            }
            if (configuration.NextMachineObservationStateId.HasValue) {
              if (configuration.NextMachineObservationStateId.Value == next.MachineObservationState.Id) {
                continue;
              }
            }
          }
          // The configuration is ok
          IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (configuration.ReasonId);
          if (null == reason) {
            log.ErrorFormat ("NotifyReasonSlotAddModify: " +
                             "reason with ID {0} does not exist",
                             configuration.ReasonId);
          }
          else {
            log.DebugFormat ("NotifyReasonSlotAddModify: " +
                             "apply reason with ID {0}",
                             reason.Id);
            slot.TryAutoReasonInReset (reason, configuration.Score, "", configuration.OverwriteRequired, slot.DateTimeRange.Upper, true);
            // TODO: try to propagate on the left...
            return true;
          }
        }
      }
      
      return false;
    }
    #endregion // IUpdateDefaultReasonListener implementation

    bool IsMachineModeIdValid (int machineModeId, IMachineMode machineMode)
    {
      return IsMachineModeIdValid (machineModeId, machineMode, log);
    }

    public static bool IsMachineModeIdValid (int machineModeId, IMachineMode machineMode, ILog log)
    {
      if (machineMode is null) {
        log.Error ($"IsMachineModeIdValid: machine mode null unexpected");
        Debug.Assert (null != machineMode);
        throw new NullReferenceException ("Unexpected null machine mode");
      }

      var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (machineModeId, machineMode);
      return Lemoine.Business.ServiceProvider
        .Get (isDescendantOrSelfOfRequest);
    }

    bool IsSameShift (IObservationStateSlot s1, IObservationStateSlot s2)
    {
      if (null == s1.Shift) {
        return (null == s2.Shift);
      }
      else {
        return (null != s2.Shift) && (s1.Shift.Id == s2.Shift.Id);
      }
    }
    
    /// <summary>
    /// Load the machine observation state slot
    /// </summary>
    /// <param name="range"></param>
    void Load (UtcDateTimeRange range)
    {
      if (!CheckCacheValidity (range)) { // Load it
        m_isCurrentObservationStateSlot = false;
        m_observationStateSlot = null;
        
        var observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindOverlapsRange (m_machine, range)
          .Where (s => (null != s.MachineObservationState))
          .Where (s => s.DateTimeRange.ContainsRange (range));
        Debug.Assert (observationStateSlots.Count () <= 1);
        m_observationStateSlot = observationStateSlots.FirstOrDefault ();
      }
      
      // If null == m_observationStateSlot is null, the machine state template was not computed yet
      // and the process is postponed
    }
    
    /// <summary>
    /// If true is returned, m_observationStateSlot is not null
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    bool CheckCacheValidity (UtcDateTimeRange range)
    {
      if (null == m_observationStateSlot) {
        return false;
      }
      
      if (!m_observationStateSlot.DateTimeRange.ContainsRange (range)) {
        return false;
      }
      
      return true;
    }
    
    void CleanCache ()
    {
      m_observationStateSlot = null;
      m_isCurrentObservationStateSlot = false;
    }

    class TemporaryCache: IDisposable
    {
      readonly ActivityAnalysisExtension m_parent;
      readonly IObservationStateSlot m_oldObservationStateSlot;
      readonly bool m_oldIsCurrentObservationStateSlot;
      
      public TemporaryCache (ActivityAnalysisExtension parent)
      {
        m_parent = parent;
        m_oldObservationStateSlot = m_parent.m_observationStateSlot;
        m_oldIsCurrentObservationStateSlot = m_parent.m_isCurrentObservationStateSlot;
      }
      
      #region IDisposable implementation
      public void Dispose()
      {
        // Restore only the old value if it was the current observation state slot
        if (!m_oldIsCurrentObservationStateSlot) {
          return;
        }
        
        if (null != m_oldObservationStateSlot) {
          m_parent.m_observationStateSlot = m_oldObservationStateSlot;
          m_parent.m_isCurrentObservationStateSlot = m_oldIsCurrentObservationStateSlot;
        }
      }
      #endregion
      
    }
  }
}
