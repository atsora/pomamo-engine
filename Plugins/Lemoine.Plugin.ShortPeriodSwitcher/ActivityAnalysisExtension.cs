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

namespace Lemoine.Plugin.ShortPeriodSwitcher
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

    bool m_currentManualActivity = false;
    IMachineMode m_currentMachineMode;
    DateTime m_currentReasonSlotEnd;

    TimeSpan m_maxDuration;
    IMachineFilter m_machineFilter = null;
    IMachineMode m_oldMachineMode; // Not null
    IMachineMode m_newMachineMode; // Not null
    bool m_oldMachineModeHasChildren = false;

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

        int oldMachineModeId = configuration.OldMachineModeId;
        Debug.Assert (0 != oldMachineModeId);
        m_oldMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById (oldMachineModeId);
        if (null == m_oldMachineMode) {
          log.ErrorFormat ("Initialize: " +
                           "oldMachineModeFilter {0} could not be loaded",
                           oldMachineModeId);
          return false;
        }
        {
          var machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO.FindAll ();
          m_oldMachineModeHasChildren = machineModes
            .Any (m => (null != m.Parent) && (m.Parent.Id == oldMachineModeId));
        }

        int newMachineModeId = configuration.NewMachineModeId;
        Debug.Assert (0 != newMachineModeId);
        m_newMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById (newMachineModeId);
        if (null == m_newMachineMode) {
          log.ErrorFormat ("Initialize: " +
                           "newMachineModeFilter {0} could not be loaded",
                           newMachineModeId);
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
    }

    /// <summary>
    /// Before procesing a new activity period
    /// </summary>
    /// <param name="machineStatus"></param>
    public void BeforeProcessingNewActivityPeriod (IMachineStatus machineStatus)
    {
      if (null == machineStatus) {
        m_currentManualActivity = false;
        m_currentMachineMode = null;
      }
      else {
        m_currentManualActivity = machineStatus.ManualActivity;
        m_currentMachineMode = machineStatus.CncMachineMode;
        m_currentReasonSlotEnd = machineStatus.ReasonSlotEnd;
      }
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
      if (null == m_currentMachineMode) {
        log.DebugFormat ("AfterProcessNewActivityPeriod: " +
                         "m_currentMachineMode null");
        return;
      }

      if (null == m_oldMachineMode) {
        log.FatalFormat ("AfterProcessNewActivityPeriod: " +
                         "m_oldMachineModeFilter is null");
        Debug.Assert (null != m_oldMachineMode);
        return;
      }

      if (m_currentManualActivity) {
        return;
      }
      if (m_currentMachineMode.Id == machineMode.Id) { // No machine mode change
        return;
      }

      // - Check machineStatus corresponds to the machine mode to update
      if ( (m_currentMachineMode.Id != m_oldMachineMode.Id)) {
        if (!m_oldMachineModeHasChildren) {
          return;
        }
        else {
          var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (m_oldMachineMode, m_currentMachineMode);
          var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
            .Get (isDescendantOrSelfOfRequest);
          if (!isDescendantOrSelfOf) {
            return;
          }
        }
      }

      // - Check the new activity does not correspond to old machine mode filter
      if (IsMatchOldMachineMode (machineMode)) {
        return;
      }

      // - Get the previous period
      DateTime periodBegin = m_currentReasonSlotEnd;
      DateTime periodEnd = m_currentReasonSlotEnd;
      while (true) {
        IReasonSlot reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindWithEnd (m_machine, periodBegin);
        if (null == reasonSlot) {
          break;
        }
        else { // null != reasonSlot
          if (IsMatchOldMachineMode (reasonSlot.MachineMode)) {
            Debug.Assert (reasonSlot.BeginDateTime.HasValue);
            periodBegin = reasonSlot.BeginDateTime.Value;
          }
          else {
            break;
          }
        }
      }

      // - Check the period is not too long
      TimeSpan duration = periodEnd.Subtract (periodBegin);
      if (m_maxDuration < duration) {
        log.DebugFormat ("AfterProcessNewActivityPeriod: " +
                         "the period is too long: {0}",
                         duration);
        return;
      }

      IActivityManual modification = ModelDAOHelper.ModelFactory
        .CreateActivityManual (m_machine, m_newMachineMode,
                               new UtcDateTimeRange (periodBegin, periodEnd));
      modification.Auto = true;
      ModelDAOHelper.DAOFactory.ActivityManualDAO
        .MakePersistent (modification);
    }

    bool IsMatchOldMachineMode (IMachineMode machineMode)
    {
      if (machineMode.Id == m_oldMachineMode.Id) {
        return true;
      }

      if (m_oldMachineModeHasChildren) {
        var attachedMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (machineMode.Id);
        Debug.Assert (null != attachedMachineMode);
        return attachedMachineMode.IsDescendantOrSelfOf (m_oldMachineMode);
      }

      return false;
    }

    /// <summary>
    /// Run before the commit of the Activities transaction
    /// </summary>
    public void BeforeActivitiesCommit ()
    {
      m_currentManualActivity = false;
      m_currentMachineMode = null;
    }

    /// <summary>
    /// Run after a rollback of the Activities transaction
    /// </summary>
    public void AfterActivitiesRollback ()
    {
      m_currentManualActivity = false;
      m_currentMachineMode = null;
    }
    #endregion // IActivityAnalysisExtension implementation
  }
}
