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
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonBreak
{
  /// <summary>
  /// 
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    TimeSpan m_margin;
    IMachineObservationState m_machineObservationState;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension ()
      : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.Break")
    {
    }

    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_margin = configuration.Margin;

      int machineObservationStateId = configuration.MachineObservationStateId;
      Debug.Assert (0 != machineObservationStateId);
      m_machineObservationState = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
        .FindById (machineObservationStateId);
      if (null == m_machineObservationState) {
        log.ErrorFormat ("Initialize: " +
                         "machine ObservationState id {0} does not exist",
                         machineObservationStateId);
        return false;
      }

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
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.Break.Check")) {
          var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
            .FindById (this.Machine.Id);
          if (null == machineStatus) {
            log.ErrorFormat ("Run: no machine status for machine {0}", this.Machine.Id);
            return;
          }
          UtcDateTimeRange range = new UtcDateTimeRange (this.DateTime, machineStatus.ReasonSlotEnd);
          if (!range.IsEmpty ()) {
            var nextBreakObservationStateSlotInRange = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (this.Machine, range)
              .Where (s => s.MachineObservationState.Id == m_machineObservationState.Id)
              .FirstOrDefault ();
            if (null == nextBreakObservationStateSlotInRange) {
              AddUpdateDateTimeDelayedAction (machineStatus.ReasonSlotEnd);
            }
            else { // null != nextBreakObservationStateSlot
              RunInBreak (range, nextBreakObservationStateSlotInRange);
            }
          }
        }
      }
    }

    void RunInBreak (UtcDateTimeRange range, IObservationStateSlot breakObservationStateSlot)
    {
      // Note: range.Upper corresponds to machineStatus.ReasonSlotEnd

      // Lower and Upper considering the margins
      UpperBound<DateTime> upper;
      if (breakObservationStateSlot.DateTimeRange.Upper.HasValue
        && breakObservationStateSlot.DateTimeRange.Duration.HasValue) {
        upper = breakObservationStateSlot.DateTimeRange.Upper.Value
          .Add (breakObservationStateSlot.DateTimeRange.Duration.Value);
      }
      else {
        upper = breakObservationStateSlot.DateTimeRange.Upper;
      }

      // Check the reasonslots in breakObservationStateSlot
      var breakIntersectionRange = new UtcDateTimeRange (breakObservationStateSlot.DateTimeRange.Intersects (range));
      Debug.Assert (!breakIntersectionRange.IsEmpty ());
      var breakReasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAllInUtcRangeWithMachineMode (this.Machine, breakIntersectionRange)
        .Where (s => Bound.Compare<DateTime> (breakIntersectionRange.Lower, s.DateTimeRange.Lower) <= 0);
      var notRunningBreakReasonSlots = breakReasonSlots
        .Where (s => s.NotRunning);

      if (notRunningBreakReasonSlots.Any ()) {
        var applicableDateTime = breakIntersectionRange.Upper.Value;

        if (notRunningBreakReasonSlots.First ().DateTimeRange.Lower.Equals (breakObservationStateSlot.DateTimeRange.Lower)) {
          ProcessFirstNotRunningReasonSlot (breakIntersectionRange, notRunningBreakReasonSlots.First (), upper);
          ProcessOtherNotRunningReasonSlots (notRunningBreakReasonSlots.Skip (1), upper);
        }
        else {
          ProcessOtherNotRunningReasonSlots (notRunningBreakReasonSlots, upper);
        }
        AddUpdateDateTimeDelayedAction (applicableDateTime);
      }
      else {
        Debug.Assert (breakIntersectionRange.Upper.HasValue);
        AddUpdateDateTimeDelayedAction (breakIntersectionRange.Upper.Value);
      }
    }

    bool CheckSameMachineModeBefore (UtcDateTimeRange breakIntersectionRange, IReasonSlot firstNotRunningReasonSlot, out DateTime sameMachineModePeriodStart)
    {
      Debug.Assert (null != firstNotRunningReasonSlot);
      Debug.Assert (firstNotRunningReasonSlot.NotRunning);
      Debug.Assert (firstNotRunningReasonSlot.DateTimeRange.Lower.HasValue);

      sameMachineModePeriodStart = firstNotRunningReasonSlot.DateTimeRange.Lower.Value;

      Debug.Assert (breakIntersectionRange.Lower.HasValue);
      var beforeRange = new UtcDateTimeRange (breakIntersectionRange.Lower.Value.Subtract (m_margin), breakIntersectionRange.Lower.Value);
      var beforeReasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRangeDescending (this.Machine, beforeRange, m_margin);
      foreach (var beforeReasonSlot in beforeReasonSlots) {
        if (beforeReasonSlot.MachineMode.Id == firstNotRunningReasonSlot.MachineMode.Id) {
          sameMachineModePeriodStart = beforeReasonSlot.DateTimeRange.Lower.Value;
        }
        else {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Extend the period of the first not running reason slot if applicable
    /// and check there is a machine mode change in the margin
    /// </summary>
    /// <param name="breakIntersectionRange"></param>
    /// <param name="firstNotRunningReasonSlot"></param>
    /// <param name="upper"></param>
    void ProcessFirstNotRunningReasonSlot (UtcDateTimeRange breakIntersectionRange, IReasonSlot firstNotRunningReasonSlot, UpperBound<DateTime> upper)
    {
      DateTime sameMachineModePeriodStart;
      if (!CheckSameMachineModeBefore (breakIntersectionRange, firstNotRunningReasonSlot, out sameMachineModePeriodStart)) {
        ApplyReason (new UtcDateTimeRange (sameMachineModePeriodStart, upper));
      }
      else {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ProcessFirstNotRunningReasonSlot: machine mode change before margin");
        }
      }
    }

    void ProcessOtherNotRunningReasonSlots (IEnumerable<IReasonSlot> reasonSlots, UpperBound<DateTime> upper)
    {
      foreach (var reasonSlot in reasonSlots) {
        SetActive ();
        var range = new UtcDateTimeRange (reasonSlot.DateTimeRange.Lower,
          upper);
        ApplyReason (range);
      }
    }

    void ApplyReason (UtcDateTimeRange range)
    {
      var action = new ApplyReasonDynamicEndBeforeRealEndAction (this, range, ",NextMachineMode", "Break (auto)");
      AddDelayedAction (action);
    }

  }
}
