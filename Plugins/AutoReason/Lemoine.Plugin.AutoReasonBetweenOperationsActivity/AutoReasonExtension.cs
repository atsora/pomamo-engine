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

namespace Lemoine.Plugin.AutoReasonBetweenOperationsActivity
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

    int m_previousOperationId; // -1 is not initialized

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension ()
      : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.BetweenOperationsActivity")
    {
    }

    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_margin = configuration.Margin;

      var previousOperationIdState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .GetAutoReasonState (this.Machine, GetKey (PREVIOUS_OPERATION_ID_KEY));
      m_previousOperationId = (previousOperationIdState != null) ? (int)previousOperationIdState.Value : -1;

      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    void AddUpdatePreviousOperationIdAction (int previousOperationId)
    {
      var action = new UpdatePreviousOperationIdAction (this, previousOperationId);
      AddDelayedAction (action);
    }

    internal int GetPreviousOperationId ()
    {
      return m_previousOperationId;
    }

    internal void UpdatePreviousOperationId (int previousOperationId)
    {
      m_previousOperationId = previousOperationId;
      var dateTimeState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .Save (this.Machine, GetKey (PREVIOUS_OPERATION_ID_KEY), previousOperationId);
    }

    internal void ResetPreviousOperationId (int id)
    {
      m_previousOperationId = id;
    }

    void GoOn (DateTime dateTime)
    {
      // TODO: check +1s
      if (dateTime <= this.DateTime) {
        dateTime = this.DateTime.AddSeconds (1);
      }
      AddUpdateDateTimeDelayedAction (dateTime);
    }

    void InitializePreviousOperationId (DateTime operationDetectionDateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        GetLogger ().InfoFormat ("Initializing the previous operation ID");
        var nextOperationSlotWithOperation = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRangeAscending (this.Machine, new UtcDateTimeRange (this.DateTime, operationDetectionDateTime), TimeSpan.FromDays (1))
          .FirstOrDefault (s => (s.Operation != null));
        if (null != nextOperationSlotWithOperation) {
          Debug.Assert (null != nextOperationSlotWithOperation.Operation);
          Debug.Assert (nextOperationSlotWithOperation.DateTimeRange.Upper.HasValue);
          Debug.Assert (this.DateTime < nextOperationSlotWithOperation.DateTimeRange.Upper.Value);
          AddUpdatePreviousOperationIdAction (((IDataWithId)nextOperationSlotWithOperation.Operation).Id);
          var dateTime = Bound.GetMinimum<DateTime> (operationDetectionDateTime, nextOperationSlotWithOperation.DateTimeRange.Upper);
          Debug.Assert (dateTime.HasValue);
          AddUpdateDateTimeDelayedAction (dateTime.Value);
        }
      }
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        var operationDetectionStatusRequest = new Lemoine.Business.Operation
          .OperationDetectionStatus (this.Machine);
        var operationDetectionStatus = Lemoine.Business.ServiceProvider
          .Get (operationDetectionStatusRequest);
        if (!operationDetectionStatus.HasValue) {
          return;
        }
        var operationDetectionDateTime = operationDetectionStatus.Value;
        if (operationDetectionDateTime < this.DateTime) {
          return;
        }

        if (-1 == m_previousOperationId) {
          InitializePreviousOperationId (operationDetectionDateTime);
          return;
        }

        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.BetweenOperationsActivity.Check")) {

          var nextOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRangeAscending (this.Machine, new UtcDateTimeRange (this.DateTime), TimeSpan.FromDays (1))
            .FirstOrDefault (s => null != s.Operation);
          if (null == nextOperationSlot) {
            GetLogger ().DebugFormat ("Run: no next operation slot with an operation after {0}", this.DateTime);
          }
          else if (((IDataWithId)nextOperationSlot.Operation).Id != m_previousOperationId) {
            var noOperationRange = new UtcDateTimeRange (this.DateTime, nextOperationSlot.DateTimeRange.Lower.Value);

            // Check that data are really computed on this range
            if (IsOperationRangeOk (noOperationRange)) {
              ProcessPeriodeBetweenOperations (noOperationRange, nextOperationSlot);
              AddUpdatePreviousOperationIdAction (((IDataWithId)nextOperationSlot.Operation).Id);
              GoOn (nextOperationSlot.DateTimeRange.Lower.Value);
            }
          }

          transaction.Commit ();
        }
      }
    }

    bool IsOperationRangeOk (UtcDateTimeRange range)
    {
      var detectionDateTime = ServiceProvider.Get<DateTime?> (new Lemoine.Business.Operation.OperationDetectionStatus (this.Machine));
      if (!detectionDateTime.HasValue) {
        GetLogger ().ErrorFormat ("Run: no operation detection status, give up");
        return false;
      }
      range = new UtcDateTimeRange (range.Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), detectionDateTime.Value)));
      if (range.IsEmpty ()) {
        GetLogger ().DebugFormat ("Run: empty range after detection date/time {0}, give up", detectionDateTime);
        return false;
      }

      return true;
    }

    void ProcessPeriodeBetweenOperations (UtcDateTimeRange range, IOperationSlot nextOperationSlot)
    {
      Debug.Assert (nextOperationSlot != null);
      if (range.Lower.Value >= range.Upper.Value) {
        return; // Nothing to do
      }

      // Details
      var details = GetDetails (this.Machine, range, nextOperationSlot);

      // Initial limits and reasonslots
      var leftInitialLimit = range.Lower.Value;
      var rightInitialLimit = range.Upper.Value;
      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO.FindOverlapsRange (this.Machine, range);

      // Margins can apply?
      var leftWithMargin = leftInitialLimit.Add (m_margin);
      var rightWithMargin = rightInitialLimit.Subtract (m_margin);
      if (leftWithMargin < rightWithMargin) {
        // Take the margin into account

        // Analyse the range
        DateTime? lastActivityDate = null;
        DateTime? firstActivityDate = null;
        foreach (var reasonSlot in reasonSlots) {
          // Check there is a start and an end
          if (!reasonSlot.BeginDateTime.HasValue || !reasonSlot.EndDateTime.HasValue) {
            continue;
          }

          var start = reasonSlot.BeginDateTime.Value;
          var end = reasonSlot.EndDateTime.Value;

          if (reasonSlot.Running) {
            if (end <= leftWithMargin && (!lastActivityDate.HasValue || end > lastActivityDate.Value)) {
              lastActivityDate = end;
            }

            if (start >= rightWithMargin && (!firstActivityDate.HasValue || firstActivityDate.Value > start)) {
              firstActivityDate = start;
            }
          }
        }
        if (!lastActivityDate.HasValue) {
          lastActivityDate = leftInitialLimit;
        }

        if (!firstActivityDate.HasValue) {
          firstActivityDate = rightInitialLimit;
        }

        // Determine if there is activity inside
        bool activityInside = false;
        if (lastActivityDate.Value >= firstActivityDate.Value) {
          activityInside = true;
        }
        else {
          foreach (var reasonSlot in reasonSlots) {
            if (!reasonSlot.BeginDateTime.HasValue || !reasonSlot.EndDateTime.HasValue) {
              continue;
            }

            var start = reasonSlot.BeginDateTime.Value;
            var end = reasonSlot.EndDateTime.Value;

            if (reasonSlot.Running &&
              (!firstActivityDate.HasValue || reasonSlot.BeginDateTime.Value < firstActivityDate.Value) &&
              (!lastActivityDate.HasValue || reasonSlot.EndDateTime.Value > lastActivityDate.Value)) {
              activityInside = true;
              break;
            }
          }
        }

        // Apply reasons if there is activity
        if (!activityInside) {
          return;
        }

        DateTime? startReason = null;
        DateTime? endReason = null;
        foreach (var reasonSlot in reasonSlots) {
          SetActive ();
          if (!reasonSlot.BeginDateTime.HasValue || !reasonSlot.EndDateTime.HasValue) {
            continue;
          }

          // Adapt start and end if running
          var start = reasonSlot.BeginDateTime.Value;
          var end = reasonSlot.EndDateTime.Value;
          if (reasonSlot.Running) {
            if (start < lastActivityDate.Value) {
              start = lastActivityDate.Value;
            }

            if (end > firstActivityDate.Value) {
              end = firstActivityDate.Value;
            }
          }

          // Adapt reason range
          if (start < end) {
            if (!startReason.HasValue || startReason.Value > start) {
              startReason = start;
            }

            if (!endReason.HasValue || endReason.Value < end) {
              endReason = end;
            }
          }
        }

        if (startReason.HasValue && endReason.HasValue && startReason.Value < endReason.Value) {
          AddReason (new UtcDateTimeRange (startReason.Value, endReason.Value), details);
        }
      }
      else {
        // No margin can be applied

        // Find at least one running period
        if (!reasonSlots.Where (s => s.Running).Any ()) {
          return;
        }

        DateTime? startReason = null;
        DateTime? endReason = null;
        foreach (var reasonSlot in reasonSlots) {
          SetActive ();
          if (!reasonSlot.BeginDateTime.HasValue || !reasonSlot.EndDateTime.HasValue) {
            continue;
          }

          // Adapt start and end if running
          var start = reasonSlot.BeginDateTime.Value;
          var end = reasonSlot.EndDateTime.Value;
          if (reasonSlot.Running) {
            if (start < leftInitialLimit) {
              start = leftInitialLimit;
            }

            if (end > rightInitialLimit) {
              end = rightInitialLimit;
            }
          }

          // Adapt reason range
          if (start < end) {
            if (!startReason.HasValue || startReason.Value > start) {
              startReason = start;
            }

            if (!endReason.HasValue || endReason.Value < end) {
              endReason = end;
            }
          }
        }

        if (startReason.HasValue && endReason.HasValue && startReason.Value < endReason.Value) {
          AddReason (new UtcDateTimeRange (startReason.Value, endReason.Value), details);
        }
      }
    }

    string GetDetails (IMachine machine, UtcDateTimeRange range, IOperationSlot nextOperationSlot)
    {
      IOperationSlot previousOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindWithEnd (this.Machine, range.Lower.Value);

      return (previousOperationSlot == null) ?
        string.Format ("Before {0}", nextOperationSlot.Display) :
        string.Format ("Between {0} and {1}", previousOperationSlot.Display, nextOperationSlot.Display);
    }

    void AddReason (UtcDateTimeRange range, string details)
    {
      var action = new ApplyReasonAction (this, range, ",NextMachineMode+", details);
      AddDelayedAction (action);
    }
  }
}
