// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonRestartAfterBreak
{
  /// <summary>
  /// 
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    IMachineObservationState m_machineObservationState;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension ()
      : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.RestartAfterBreak")
    {
    }

    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      int machineObservationStateId = configuration.MachineObservationStateId;
      Debug.Assert (machineObservationStateId >= 0);
      m_machineObservationState = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById (machineObservationStateId);
      if (null == m_machineObservationState) {
        log.ErrorFormat ("Initialize: machine observation state id {0} does not exist", machineObservationStateId);
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
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.RestartAfterBreak.Check")) {

          // Status of the current machine
          var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (this.Machine.Id);
          if (machineStatus == null) {
            log.ErrorFormat ("Run: no machine status for machine {0}", this.Machine.Id);
            return;
          }

          // Is it possible to analyze data?
          UtcDateTimeRange range = new UtcDateTimeRange (this.DateTime, machineStatus.ReasonSlotEnd);
          if (!range.IsEmpty ()) {

            // Get the next break in the range
            var nextBreakObservationStateSlotInRange = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (this.Machine, range)
              .Where (s => s.MachineObservationState.Id == m_machineObservationState.Id)
              .FirstOrDefault ();
            if (null == nextBreakObservationStateSlotInRange) {
              // No break in the specified range => we go on
              AddUpdateDateTimeDelayedAction (machineStatus.ReasonSlotEnd);
            }
            else if (nextBreakObservationStateSlotInRange.EndDateTime.HasValue &&
            nextBreakObservationStateSlotInRange.EndDateTime.Value < machineStatus.ReasonSlotEnd) {
              // The break found is finished => we process it
              ProcessBreak (nextBreakObservationStateSlotInRange);
              AddUpdateDateTimeDelayedAction (nextBreakObservationStateSlotInRange.EndDateTime.Value);
            }
            else {
              // Break not finished => we do nothing
            }
          }
        }
      }
    }

    void ProcessBreak (IObservationStateSlot breakObservationStateSlot)
    {
      // Start a reason at the end of the break
      Debug.Assert (breakObservationStateSlot.DateTimeRange.Upper.HasValue);
      var range = new UtcDateTimeRange (breakObservationStateSlot.DateTimeRange.Upper.Value);
      var action = new ApplyReasonAction (this, range, ",NextMachineMode", "Restart after break");
      AddDelayedAction (action);
    }
  }
}
