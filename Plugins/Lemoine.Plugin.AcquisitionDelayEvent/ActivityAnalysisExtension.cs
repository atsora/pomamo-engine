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

namespace Lemoine.Plugin.AcquisitionDelayEvent
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

    DateTime m_lastEventCreationDateTime = DateTime.UtcNow;

    TimeSpan m_maxDuration;
    IMachineFilter m_machineFilter = null;
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
        log.Warn ("Initialize: the configuration is not valid");
        return false;
      }

      m_maxDuration = configuration.MaxDuration;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineFilterId = configuration.MachineFilterId;
        if (0 != machineFilterId) {
          m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (machineFilterId);
          if (null == m_machineFilter) {
            log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
            return false;
          }
        }

        int eventLevelId = configuration.EventLevelId;
        Debug.Assert (0 != eventLevelId);
        m_eventLevel = ModelDAOHelper.DAOFactory.EventLevelDAO
          .FindById (eventLevelId);
        if (null == m_eventLevel) {
          log.Error ($"Initialize: event level {eventLevelId} could not be loaded");
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

    /// <summary>
    /// Before processing the activities
    /// </summary>
    /// <param name="lastActivityAnalysisDateTime"></param>
    /// <param name="facts"></param>
    public void BeforeProcessingActivities (DateTime lastActivityAnalysisDateTime, IList<IFact> facts)
    {
      var now = DateTime.UtcNow;
      if (!facts.Any ()) {
        DateTime limitDateTime = lastActivityAnalysisDateTime.Add (m_maxDuration);
        if ( (limitDateTime <= now)
          && (m_lastEventCreationDateTime < limitDateTime)) {
          var range = new UtcDateTimeRange (lastActivityAnalysisDateTime, now);
          CreateEvent (range);
          m_lastEventCreationDateTime = now;
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
        using (var transaction = session.BeginTransaction ("Plugin.AcquisitionDelayEvent.CreateEvent", TransactionLevel.ReadCommitted)) {
          log.InfoFormat ($"CreateEvent: period {range} detected");
          var eventLevel = m_eventLevel;
          var eventAcquisitionDelay = new EventAcquisitionDelay (eventLevel, DateTime.UtcNow, m_machine, range);
          new EventAcquisitionDelayDAO ().MakePersistent (eventAcquisitionDelay);
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
