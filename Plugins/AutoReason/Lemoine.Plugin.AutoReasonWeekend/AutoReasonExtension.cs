// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonWeekend
{
  /// <summary>
  /// Detect weekends and apply an auto reason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    IMachineObservationState m_machineObservationState; // Configured to be weekend
    TimeSpan m_endWeekendMargin = TimeSpan.FromHours (1);

    // First date where we found a weekend observation state
    static readonly string WEEKEND_START_DATE = "WeekendStartDate";
    DateTime? m_weekendStartDate = null;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.Weekend")
    {
    }

    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_endWeekendMargin = configuration.Margin;
      int machineObservationStateId = configuration.MachineObservationStateId;
      Debug.Assert (0 != machineObservationStateId);

      // Check that the id exists
      m_machineObservationState = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById (machineObservationStateId);
      if (null == m_machineObservationState) {
        log.ErrorFormat ("Initialize: " +
                         "machine ObservationState id {0} does not exist",
                         machineObservationStateId);
        return false;
      }

      // Start date of the weekend
      var weekendStartDate = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (this.Machine, GetKey (WEEKEND_START_DATE));
      m_weekendStartDate = (weekendStartDate != null) ? (DateTime?)weekendStartDate.Value : null;

      return true;
    }

    public override bool CanOverride (IReasonSlot reasonSlot)
    {
      if (!base.CanOverride (reasonSlot)) {
        return false;
      }

      return reasonSlot.NotRunning;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return reasonSlot.NotRunning;
    }

    public override bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score)
    {
      if (!base.IsValidMatch (machineMode, machineObservationState, reason, score)) {
        return false;
      }

      return machineMode.Running.HasValue && !machineMode.Running.Value;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.Weekend.Check")) {

          // Check that data have been computed for the current machine
          var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (this.Machine.Id);
          if (machineStatus == null) {
            log.ErrorFormat ("RunOnce: no machine status for machine {0}", this.Machine.Id);
            return;
          }

          // In the period where data have been computed, starting from the current date
          UtcDateTimeRange range = new UtcDateTimeRange (this.DateTime, machineStatus.ReasonSlotEnd);
          if (!range.IsEmpty ()) {
            // Get the next period
            var nextObservationStateSlotInRange = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
                .FindOverlapsRange (this.Machine, range)
                .FirstOrDefault ();

            if (nextObservationStateSlotInRange == null || !nextObservationStateSlotInRange.DateTimeRange.Lower.HasValue ||
               !nextObservationStateSlotInRange.DateTimeRange.Upper.HasValue) {
              // Nothing is currently happening
              AddUpdateDateTimeDelayedAction (machineStatus.ReasonSlotEnd);
            }
            else {
              // We found a new period
              if (m_weekendStartDate.HasValue) {
                if (nextObservationStateSlotInRange.MachineObservationState.Id != m_machineObservationState.Id) {
                  // The weekend stopped, we process the full period
                  ProcessWeekend (m_weekendStartDate.Value, nextObservationStateSlotInRange.DateTimeRange.Lower.Value);
                  UpdateWeekendStartDate (null);
                }
              }
              else {
                if (nextObservationStateSlotInRange.MachineObservationState.Id == m_machineObservationState.Id) {
                  // Great, the weekend begins!
                  Debug.Assert (nextObservationStateSlotInRange.DateTimeRange.Lower.HasValue);
                  UpdateWeekendStartDate (nextObservationStateSlotInRange.DateTimeRange.Lower.Value);
                }
              }

              // Go on in all cases
              AddUpdateDateTimeDelayedAction (nextObservationStateSlotInRange.DateTimeRange.Upper.Value);
            }
          }
        }
      }
    }

    void ProcessWeekend (DateTime weekendStart, DateTime weekendEnd)
    {
      // Analyze all reason slots during the weekend
      var weekendReasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAllInUtcRangeWithMachineMode (this.Machine, new UtcDateTimeRange (weekendStart, weekendEnd));

      // Start from the end, get the longest non-running period as possible
      DateTime? nonRunningStart = null;
      DateTime? nonRunningEnd = null;
      for (int i = weekendReasonSlots.Count - 1; i >= 0; i--) {
        var reasonSlot = weekendReasonSlots[i];
        if (reasonSlot.NotRunning) {
          if (!nonRunningEnd.HasValue) {
            // We found the end of the non running period during the weekend
            if (reasonSlot.EndDateTime.HasValue) {
              nonRunningEnd = reasonSlot.EndDateTime.Value;
            }
            else {
              nonRunningEnd = weekendEnd;
            }
          }
        }
        else {
          // Process only if the end of the non running period is already found
          if (nonRunningEnd.HasValue) {
            // We found the start of the last non running period during the weekend
            if (reasonSlot.EndDateTime.HasValue) {
              nonRunningStart = reasonSlot.EndDateTime.Value;
              break; // Stop the iteration here
            }
            else {
              // Shouldn't happen
              log.FatalFormat ("Weekend autoreason: found reasonslot id {0} with no end but followed by another reasonslot", reasonSlot.Id);
              return;
            }
          }
        }
      }

      if (!nonRunningEnd.HasValue) {
        return; // everything was "running", nothing else to do
      }

      if (!nonRunningStart.HasValue) // everything was "not running"
{
        nonRunningStart = weekendStart;
      }

      // Keep the limits within the weekend
      if (nonRunningStart.Value < weekendStart) {
        nonRunningStart = weekendStart;
      }

      if (nonRunningEnd.Value > weekendEnd) {
        nonRunningEnd = weekendEnd;
      }

      // Check that both limits are not the same
      if (nonRunningStart.Value == nonRunningEnd.Value) {
        return;
      }

      // Check that the end of the last non running period during the weekend is close to the end of the weekend, considering a margin
      if (nonRunningEnd.Value < weekendEnd.Subtract (m_endWeekendMargin)) {
        return;
      }

      // It's ok for creating an auto reason
      CreateAutoReason (new UtcDateTimeRange (nonRunningStart.Value, nonRunningEnd.Value));
    }

    void CreateAutoReason (UtcDateTimeRange range)
    {
      var action = new ApplyReasonAction (this, range);
      AddDelayedAction (action);
    }

    void UpdateWeekendStartDate (DateTime? weekendStartDate)
    {
      m_weekendStartDate = weekendStartDate;
      log.InfoFormat ("AutoReasonWeekend: updated '{0}' to '{1}'", GetKey (WEEKEND_START_DATE), m_weekendStartDate);
      if (weekendStartDate.HasValue) {
        ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (WEEKEND_START_DATE), m_weekendStartDate);
      }
      else {
        ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Remove (this.Machine, GetKey (WEEKEND_START_DATE));
      }
    }
  }
}
