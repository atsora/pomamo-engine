// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ShortPeriodRemoval
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
    IMachineMode m_oldMachineModeFilter; // Not null
    IMachineMode m_newMachineModeFilter = null;

    #region Getters / Setters
    /// <summary>
    /// New machine mode filter (for the unit tests)
    /// </summary>
    public IMachineMode NewMachineModeFilter { get { return m_newMachineModeFilter; } }
    #endregion // Getters / Setters

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
        m_oldMachineModeFilter = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById (oldMachineModeId);
        if (null == m_oldMachineModeFilter) {
          log.ErrorFormat ("Initialize: " +
                           "oldMachineModeFilter {0} could not be loaded",
                           oldMachineModeId);
          return false;
        }

        int newMachineModeId = configuration.NewMachineModeId;
        if (0 != newMachineModeId) {
          m_newMachineModeFilter = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (newMachineModeId);
          if (null == m_newMachineModeFilter) {
            log.ErrorFormat ("Initialize: " +
                             "newMachineModeFilter {0} could not be loaded",
                             newMachineModeId);
            return false;
          }
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
                         "m_previousMachineMode null");
        return;
      }

      if (null == m_oldMachineModeFilter) {
        log.FatalFormat ("AfterProcessNewActivityPeriod: " +
                         "m_oldMachineModeFilter is null");
        Debug.Assert (null != m_oldMachineModeFilter);
        return;
      }

      if (m_currentManualActivity) {
        return;
      }
      if (m_currentMachineMode.Id == machineMode.Id) { // No machine mode change
        return;
      }

      // - Check machineStatus corresponds to the machine mode to update
      if (m_currentMachineMode.Id != m_oldMachineModeFilter.Id) {
        var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (m_oldMachineModeFilter, m_currentMachineMode);
        var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
          .Get (isDescendantOrSelfOfRequest);
        if (!isDescendantOrSelfOf) {
          return;
        }
      }

      // - Check the new activity does not correspond to old machine mode filter
      if (machineMode.Id == m_oldMachineModeFilter.Id) {
        return;
      }
      else {
        var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (m_oldMachineModeFilter, machineMode);
        var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
          .Get (isDescendantOrSelfOfRequest);
        if (isDescendantOrSelfOf) {
          return;
        }
      }

      // - Check the new activity matches the filter
      if (null != m_newMachineModeFilter) {
        if (machineMode.Id != m_newMachineModeFilter.Id) {
          var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (m_newMachineModeFilter, machineMode);
          var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
            .Get (isDescendantOrSelfOfRequest);
          if (!isDescendantOrSelfOf) {
            return;
          }
        }
      }

      // - Check there is no gap
      if (Bound.Compare<DateTime> (m_currentReasonSlotEnd, activityRange.Lower) < 0) {
        log.DebugFormat ("AfterProcessNewActivityPeriod: " +
                         "gap => return");
        return;
      }

      // - Get the previous period
      DateTime periodBegin = m_currentReasonSlotEnd;
      DateTime periodEnd = m_currentReasonSlotEnd;
      IMachineMode previousMachineMode = null;
      while (true) {
        IReasonSlot reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindWithEnd (m_machine, periodBegin);
        if (null == reasonSlot) {
          break;
        }
        else { // null != reasonSlot
          if (reasonSlot.MachineMode.Id == m_oldMachineModeFilter.Id) {
            Debug.Assert (reasonSlot.BeginDateTime.HasValue);
            periodBegin = reasonSlot.BeginDateTime.Value;
          }
          else {
            var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (m_oldMachineModeFilter, reasonSlot.MachineMode);
            var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
              .Get (isDescendantOrSelfOfRequest);
            if (isDescendantOrSelfOf) {
              Debug.Assert (reasonSlot.BeginDateTime.HasValue);
              periodBegin = reasonSlot.BeginDateTime.Value;
            }
            else {
              previousMachineMode = reasonSlot.MachineMode;
              break;
            }
          }
        }
      }
      if (null == previousMachineMode) {
        log.WarnFormat ("AfterProcessNewActivityPeriod: " +
                        "no previous machine mode");
        return;
      }

      // - Check the period is not too long
      TimeSpan duration = periodEnd.Subtract (periodBegin);
      if (m_maxDuration < duration) {
        log.DebugFormat ("AfterProcessNewActivityPeriod: " +
                         "the acquisition error period is too long: {0}",
                         duration);
        return;
      }

      // - Check there is a common parent with the previous machine mode
      var attachedMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (machineMode.Id);
      Debug.Assert (null != attachedMachineMode);
      IMachineMode attachedPreviousMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
        .FindById (previousMachineMode.Id);
      IMachineMode commonAncestor = attachedMachineMode.GetCommonAncestor (attachedPreviousMachineMode);
      if (null == commonAncestor) {
        log.DebugFormat ("AfterProcessNewActivityPeriod: " +
                         "no common ancestor between {0} and {1}",
                         machineMode, previousMachineMode);
        return;
      }
      else { // Override acquisition error by ancestor
        IActivityManual modification = ModelDAOHelper.ModelFactory
          .CreateActivityManual (m_machine, commonAncestor,
                                 new UtcDateTimeRange (periodBegin, periodEnd));
        modification.Auto = true;
        ModelDAOHelper.DAOFactory.ActivityManualDAO
          .MakePersistent (modification);
      }
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
