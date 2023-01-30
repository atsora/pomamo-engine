// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.Plugin.AcquisitionErrorEvent
{
  /// <summary>
  /// Detect short acquisition errors and remove them
  /// </summary>
  public class ActivityAnalysisExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Analysis.IActivityAnalysisExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ActivityAnalysisExtension).FullName);

    IMonitoredMachine m_machine;

    DateTime? m_periodStart = null;
    bool m_periodProcessed = false;
    bool m_periodStartInitialized = false;

    TimeSpan m_maxDuration;
    IMachineFilter m_machineFilter = null;
    IMachineMode m_machineMode; // Not null
    IEventLevel m_eventLevel; // Not null

    #region IActivityAnalysisExtension implementation
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machine"></param>
    public bool Initialize (IMonitoredMachine machine)
    {
      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.WarnFormat ("Initialize: " +
                        "the configuration is not valid");
        return false;
      }

      m_maxDuration = configuration.MaxDuration;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineFilterId = configuration.MachineFilterId;
        if (0 != machineFilterId) {
          m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (machineFilterId);
          if (null == m_machineFilter) {
            log.ErrorFormat ("Initialize: " +
                             "machine filter id {0} does not exist",
                             machineFilterId);
            return false;
          }
        }

        int machineModeId = configuration.MachineModeId;
        Debug.Assert (0 != machineModeId);
        m_machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById (machineModeId);
        if (null == m_machineMode) {
          log.ErrorFormat ("Initialize: " +
                           "Machine Mode {0} could not be loaded",
                           machineModeId);
          return false;
        }

        int eventLevelId = configuration.EventLevelId;
        Debug.Assert (0 != eventLevelId);
        m_eventLevel = ModelDAOHelper.DAOFactory.EventLevelDAO
          .FindById (eventLevelId);
        if (null == m_eventLevel) {
          log.ErrorFormat ("Initialize: " +
                           "event level {0} could not be loaded",
                           eventLevelId);
          return false;
        }

        m_machine = machine;
        // Note: machineFilter.IsMatch requires it is done in the same session
        if ((null != m_machineFilter) && !m_machineFilter.IsMatch (machine)) {
          return false;
        }
      } // session

      return true;
    }

    void InitializePeriodStart (DateTime lastActivityAnalysisDateTime)
    {
      if (m_periodStartInitialized) {
        return;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.AcquisitionErrorEvent.InitializePeriodStart")) {
          m_periodStart = null;

          var lower = lastActivityAnalysisDateTime.Subtract (m_maxDuration);
          var range = new UtcDateTimeRange (lower, lastActivityAnalysisDateTime);
          var facts = ModelDAOHelper.DAOFactory.FactDAO
            .FindAllInUtcRange (m_machine, range);
          foreach (var fact in facts.Reverse ()) {
            if (fact.CncMachineMode.Id != m_machineMode.Id) {
              break;
            }
            Debug.Assert (fact.Range.Lower.HasValue);
            var duration = lastActivityAnalysisDateTime.Subtract (fact.Range.Lower.Value);
            m_periodStart = fact.Range.Lower.Value;
            if (m_maxDuration <= duration) { // duration reached, period processed
              m_periodProcessed = true;
              break;
            }
          }
        }
        m_periodStartInitialized = true;
      }
    }

    /// <summary>
    /// Before processing the activities
    /// </summary>
    /// <param name="lastActivityAnalysisDateTime"></param>
    /// <param name="facts"></param>
    public void BeforeProcessingActivities (DateTime lastActivityAnalysisDateTime, IList<IFact> facts)
    {
      InitializePeriodStart (lastActivityAnalysisDateTime);
      Debug.Assert (m_periodStartInitialized);
      foreach (var fact in facts) {
        CheckFact (fact);
      }
    }

    void CheckFact (IFact fact)
    {
      if (null == m_machineMode) {
        log.FatalFormat ("CheckFact: " +
                         "m_MachineMode is null");
        Debug.Assert (null != m_machineMode);
        return;
      }

      var machineMode = fact.CncMachineMode;
      Debug.Assert (null != machineMode);
      var activityRange = fact.Range;

      if (null == machineMode) {
        log.FatalFormat ("CheckFact: machineMode is null");
        Debug.Assert (null != machineMode);
        return;
      }

      if (machineMode.Id != m_machineMode.Id) { // new machine mode
        m_periodStart = null;
        m_periodProcessed = false;
      }
      else if (!m_periodProcessed) { // && same machine mode
        if (m_periodStart.HasValue) {
          if (Bound.Compare<DateTime> (activityRange.Upper, m_periodStart.Value) <= 0) {
            // Skip this fact
            return;
          }
          Debug.Assert (activityRange.Upper.HasValue);
          Debug.Assert (m_periodStart.Value <= activityRange.Upper.Value);
          var range = new UtcDateTimeRange (m_periodStart.Value, activityRange.Upper.Value);
          Debug.Assert (range.Duration.HasValue);
          var duration = range.Duration.Value;
          if (m_maxDuration <= duration) {
            log.InfoFormat ("CheckFact: period {0}-{1} detected for {2}",
              m_periodStart.Value, activityRange.Upper.Value, machineMode);
            CreateEvent (range);
            m_periodProcessed = true;
          }
        }
        else { // !m_periodStart.HasValue
          Debug.Assert (activityRange.Lower.HasValue);
          log.DebugFormat ("CheckFact: new acquisition error period at {0}, record it",
            activityRange.Lower.Value);
          Debug.Assert (activityRange.Duration.HasValue);
          var duration = activityRange.Duration.Value;
          if (m_maxDuration <= duration) {
            CreateEvent (activityRange);
            m_periodProcessed = true;
          }
          m_periodStart = activityRange.Lower.Value;
        }
      }
    }

    /// <summary>
    /// Before procesing a new activity period
    /// </summary>
    /// <param name="machineStatus"></param>
    public void BeforeProcessingNewActivityPeriod (IMachineStatus machineStatus)
    {
    }

    /// <summary>
    /// Process a new activity period (which is not empty)
    /// </summary>
    /// <param name="activityRange"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="shift"></param>
    public void AfterProcessingNewActivityPeriod (UtcDateTimeRange activityRange,
                                                 IMachineMode machineMode,
                                                 IMachineStateTemplate machineStateTemplate,
                                                 IMachineObservationState machineObservationState,
                                                 IShift shift)
    {
    }

    void CreateEvent (UtcDateTimeRange range)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Plugin.AcquisitionErrorEvent.CreateEvent", TransactionLevel.ReadCommitted)) {
          log.InfoFormat ("CreateEvent: period {0} detected for {2}",
            range, m_machineMode);
          var eventLevel = m_eventLevel;
          var eventAcquisitionError = new EventAcquisitionError (eventLevel, DateTime.UtcNow, m_machine, m_machineMode, range);
          new EventAcquisitionErrorDAO ().MakePersistent (eventAcquisitionError);
          transaction.Commit ();
        }
      }
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
  }
}