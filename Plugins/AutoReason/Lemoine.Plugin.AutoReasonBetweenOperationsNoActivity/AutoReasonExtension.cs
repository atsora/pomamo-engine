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
using Lemoine.Business;
using Lemoine.Extensions.AutoReason.Action;
using Lemoine.Collections;

namespace Lemoine.Plugin.AutoReasonBetweenOperationsNoActivity
{
  /// <summary>
  /// Detect short acquisition errors and remove them
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    static readonly string PREVIOUS_OPERATION_ID_KEY = "PreviousOperationId";

    TimeSpan m_margin;

    int? m_previousOperationId;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension ()
      : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.BetweenOperationsNoActivity")
    {
    }

    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_margin = configuration.Margin;

      var previousOperationIdState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .GetAutoReasonState (this.Machine, GetKey (PREVIOUS_OPERATION_ID_KEY));
      if (null != previousOperationIdState) {
        m_previousOperationId = (int)previousOperationIdState.Value;
      }

      log = LogManager
        .GetLogger (string.Format ("{0}.{1}", typeof (AutoReasonExtension).FullName, this.Machine.Id));

      return true;
    }

    public override bool CanOverride (IReasonSlot reasonSlot)
    {
      if (!base.CanOverride (reasonSlot)) {
        return false;
      }

      // For optimization... in case the machine is running,
      // this specific auto-reason won't apply anyway
      return !reasonSlot.Running;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return !reasonSlot.Running;
    }

    public override bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score)
    {
      if (!base.IsValidMatch (machineMode, machineObservationState, reason, score)) {
        return false;
      }

      return !machineMode.Running.HasValue || !machineMode.Running.Value;
    }

    void AddUpdateStateActions (IOperation operation, DateTime dateTime)
    {
      Debug.Assert (null != operation);

      AddUpdatePreviousOperationIdAction (((IDataWithId)operation).Id);
      AddUpdateDateTimeDelayedAction (dateTime);
    }

    void AddUpdatePreviousOperationIdAction (int previousOperationId)
    {
      var action = new UpdatePreviousOperationIdAction (this, previousOperationId);
      AddDelayedAction (action);
    }

    internal int? GetPreviousOperationId ()
    {
      return m_previousOperationId;
    }

    internal void UpdatePreviousOperationId (int previousOperationId)
    {
      m_previousOperationId = previousOperationId;
      var dateTimeState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .Save (this.Machine, GetKey (PREVIOUS_OPERATION_ID_KEY), previousOperationId);
    }

    internal void ResetPreviousOperationId (int? previousOperationId)
    {
      m_previousOperationId = previousOperationId;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.BetweenOperationsNoActivity.Check")) {

          if (!m_previousOperationId.HasValue) {
            GetLogger ().WarnFormat ("Run: previous operation slot has no operation at {0}, adjust date/time", this.DateTime);
            var nextOperationSlotWithOperation = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindOverlapsRangeAscending (this.Machine, new UtcDateTimeRange (this.DateTime), TimeSpan.FromDays (1))
              .FirstOrDefault (s => (null != s.Operation)
                                    && (Bound.Compare<DateTime> (this.DateTime, s.DateTimeRange.Lower) < 0));
            if (null != nextOperationSlotWithOperation) {
              Debug.Assert (null != nextOperationSlotWithOperation.Operation);
              Debug.Assert (nextOperationSlotWithOperation.DateTimeRange.Lower.HasValue);
              Debug.Assert (Bound.Compare<DateTime> (this.DateTime, nextOperationSlotWithOperation.DateTimeRange.Lower) < 0);
              AddUpdateStateActions (nextOperationSlotWithOperation.Operation,
                nextOperationSlotWithOperation.DateTimeRange.Lower.Value);
            }
            transaction.Commit ();
            return;
          }

          var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
            .FindById (this.Machine.Id);
          if (null == machineStatus) {
            GetLogger ().ErrorFormat ("Run: no machine status for machine {0}", this.Machine.Id);
            transaction.Commit ();
            return;
          }
          var range = new UtcDateTimeRange (this.DateTime, machineStatus.ReasonSlotEnd, "(]");
          if (range.IsEmpty ()) {
            GetLogger ().DebugFormat ("Run: empty range, give up");
            transaction.Commit ();
            return;
          }

          var detectionDateTime = ServiceProvider
            .Get<DateTime?> (new Lemoine.Business.Operation.OperationDetectionStatus (this.Machine));
          if (!detectionDateTime.HasValue) {
            GetLogger ().ErrorFormat ("Run: no operation detection status, give up");
            transaction.Commit ();
            return;
          }
          range = new UtcDateTimeRange (range.Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), detectionDateTime.Value)));
          if (range.IsEmpty ()) {
            GetLogger ().DebugFormat ("Run: empty range after detection date/time {0}, give up", detectionDateTime);
            transaction.Commit ();
            return;
          }

          var nextOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRangeAscending (this.Machine, range, TimeSpan.FromDays (1))
            .FirstOrDefault (s => null != s.Operation);
          if (null == nextOperationSlot) {
            GetLogger ().DebugFormat ("Run: no next operation slot with an operation after {0}", this.DateTime);
            transaction.Commit ();
            return;
          }
          if (((IDataWithId)nextOperationSlot.Operation).Id != m_previousOperationId) {
            var noOperationRange = new UtcDateTimeRange (this.DateTime, nextOperationSlot.DateTimeRange.Lower.Value);
            var noOperationRangeExcludeMargin = new UtcDateTimeRange (this.DateTime.Add (m_margin), nextOperationSlot.DateTimeRange.Lower.Value.Subtract (m_margin));
            ApplyReasonIfNoActivity (noOperationRange, noOperationRangeExcludeMargin, nextOperationSlot);
          }

          AdjustDateTime (nextOperationSlot, detectionDateTime.Value);
          transaction.Commit ();
        }
      }
    }

    void ApplyReasonIfNoActivity (UtcDateTimeRange range, UtcDateTimeRange rangeExcludeMargin, IOperationSlot nextOperationSlot)
    {
      Debug.Assert (null != nextOperationSlot);

      if (rangeExcludeMargin.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ApplyReasonIfNotActivity: rangeExcludeMargin is empty => do nothing");
        }
      }
      else if (!rangeExcludeMargin.Lower.HasValue) {
        log.FatalFormat ("ApplyReasonIfNoActivity: no lower value in rangeExcludeMargin, unexpected");
        Debug.Assert (false);
      }
      else { // !rangeExcludeMargin.IsEmpty ()
        var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRangeAscending (this.Machine, rangeExcludeMargin, TimeSpan.FromDays (1));
        var firstReasonSlotWithActivity = reasonSlots
          .FirstOrDefault (s => s.Running);
        if (null == firstReasonSlotWithActivity) {
          string details = GetDetails (this.Machine, range, nextOperationSlot);
          AddReason (GetApplicableRange (range, rangeExcludeMargin), details);
        }
      }
    }

    string GetDetails (IMachine machine, UtcDateTimeRange range, IOperationSlot nextOperationSlot)
    {
      IOperationSlot previousOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindWithEnd (this.Machine, range.Lower.Value);
      return GetDetails (previousOperationSlot, nextOperationSlot);
    }

    string GetDetails (IOperationSlot previousOperationSlot, IOperationSlot nextOperationSlot)
    {
      if (null != previousOperationSlot) {
        return string.Format ("Between {0} and {1}",
          previousOperationSlot.Display, nextOperationSlot.Display);
      }
      else {
        return string.Format ("Before {0}", nextOperationSlot.Display);
      }
    }

    UtcDateTimeRange GetApplicableRange (UtcDateTimeRange range, UtcDateTimeRange rangeExcludeMargin)
    {
      var start = rangeExcludeMargin.Lower;
      var end = rangeExcludeMargin.Upper;

      { // Try to extend on the left
        var leftReasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRangeDescending (this.Machine, new UtcDateTimeRange (range.Lower, start.Value), TimeSpan.FromDays (1));
        var firstRunningOnLeft = leftReasonSlots.FirstOrDefault (s => s.Running);
        if (null != firstRunningOnLeft) {
          Debug.Assert (firstRunningOnLeft.DateTimeRange.Upper.HasValue);
          start = firstRunningOnLeft.DateTimeRange.Upper.Value;
        }
        else {
          start = range.Lower;
        }
      }

      { // Try to extend on the right
        var rightReasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRangeAscending (this.Machine, new UtcDateTimeRange (end.Value, range.Upper), TimeSpan.FromDays (1));
        var firstRunningOnRight = rightReasonSlots.FirstOrDefault (s => s.Running);
        if (null != firstRunningOnRight) {
          Debug.Assert (firstRunningOnRight.DateTimeRange.Lower.HasValue);
          end = firstRunningOnRight.DateTimeRange.Lower.Value;
        }
        else {
          end = range.Upper;
        }
      }

      return new UtcDateTimeRange (start, end);
    }

    void AdjustDateTime (IOperationSlot operationSlot, DateTime detectionDateTime)
    {
      Debug.Assert (null != operationSlot.Operation);

      var min = Bound.GetMinimum<DateTime> (operationSlot.EndDateTime, detectionDateTime).Value;
      AddUpdateStateActions (operationSlot.Operation, min);
    }

    void AddReason (UtcDateTimeRange range, string details)
    {
      var action = new ApplyReasonAction (this, range, details: details);
      AddDelayedAction (action);
    }
  }
}
