// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business;
using System.Linq;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonUniqueMachineModeInShift
{
  /// <summary>
  /// Detect idle periods at the end of a shift and apply an auto-reason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    bool m_notRunningMachineMode = true;

    // ID of the previous shift
    static readonly string PREVIOUS_SHIFT_ID = "PreviousShiftID";
    int m_previousShiftId = -2; // -2 is not initialized, -1 will be "no shift"

    // Beginning of the shift
    static readonly string START_OF_SHIFT = "StartOfShift";
    DateTime? m_startShiftDateTime = null;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.NoRunningMachineModeInShift")
    {

    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager.GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_notRunningMachineMode = configuration.NotRunningMachineMode;
      // Everything is ok
      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      // Limit for the detection
      DateTime maxForDetection = this.DateTime;
      log.Info ($"Check: maxForDetection= {maxForDetection}");

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.NoRunningMachineModeInShift.Check")) {
          // find shift containing begin of range
          // use MachineState->MachineShiftSlot
          var dao = new Lemoine.Business.MachineState.MachineShiftSlotDAO ();
          var machineShiftSlot = dao.FindAt (Machine, (DateTime)maxForDetection);
          log.Debug ($"Check: shift={machineShiftSlot}");

          // check that shift has end
          if (!machineShiftSlot.DateTimeRange.Upper.HasValue) {
            log.Debug ($"Check: shift has no end");
            return;
          }

          // check that end of shit is before last fact
            var lastFact = ModelDAOHelper.DAOFactory.FactDAO.GetLast (Machine);
          log.Debug ($"Check: last fact={lastFact}");
          if (machineShiftSlot.DateTimeRange.Upper.HasValue && machineShiftSlot.DateTimeRange.Upper.Value > lastFact.End) {
            log.Debug ($"Check: end of shift after last fact end");
            return;
          }
          // ignore shift if no begin date
          if (!machineShiftSlot.DateTimeRange.Lower.HasValue) {
            log.Debug ($"Check: no begin value for shift, skip it");
            if (machineShiftSlot.DateTimeRange.Upper.HasValue) {
              var newDateTime = machineShiftSlot.DateTimeRange.Upper.Value;
              log.Debug ($"Check: update dateTime to {newDateTime}");
              this.AddUpdateDateTimeDelayedAction (newDateTime);
            }
            return;
          }

          // check only machine mode not production in this shift
          var facts = ModelDAOHelper.DAOFactory.FactDAO.FindAllInUtcRange (Machine, machineShiftSlot.DateTimeRange);
          bool isProduction = false;
          int machineModeId = -1;
          string machineModeDisplay = "no machine mode";
          foreach (var fact in facts) {
            if (m_notRunningMachineMode && fact.CncMachineMode.Running.HasValue && true == fact.CncMachineMode.Running.Value) {
              log.Debug ($"Check: fact is production");
              isProduction = true;
              break;
            }
            else {
              // Check if same machine mode
              if (-1 == machineModeId) {
                machineModeId = fact.CncMachineMode.Id;
                machineModeDisplay = fact.CncMachineMode.Display;
              }
              else {
                if (machineModeId != fact.CncMachineMode.Id) {
                  log.Debug ($"Check: different machine mode, consider as production");
                  isProduction = true;
                  break;
                }
              }
            }
          }
          if (isProduction) {
            log.Debug ($"Check: at least one fact in shift is production");
          }
          else {
            // create autoreason
            log.Debug ($"Check: create autoreason start {machineShiftSlot}");
            CreateAutoReason (machineShiftSlot.DateTimeRange, machineModeDisplay);
          }
          // update max for detection to end of shift
          if (machineShiftSlot.DateTimeRange.Upper.HasValue) {
            var newDateTime = machineShiftSlot.DateTimeRange.Upper.Value;
            log.Debug ($"Check: update dateTime to {newDateTime}");
            this.AddUpdateDateTimeDelayedAction (newDateTime);
          }
        }
      }
    }

    void CreateAutoReason (UtcDateTimeRange range, string machineModeDisplay)
    {
      var action = new ApplyReasonAction (this, range, "", "Machine mode: " + machineModeDisplay);
      AddDelayedAction (action);
    }

    void AddUpdateShiftAction (int previousShiftId, DateTime? startOfShift)
    {
      var action = new UpdateShiftStateAction (this, previousShiftId, startOfShift);
      AddDelayedAction (action);
    }

    internal int GetPreviousShiftId ()
    {
      return m_previousShiftId;
    }

    internal void UpdatePreviousShiftId (int id)
    {
      m_previousShiftId = id;
      log.InfoFormat ("AutoReasonUniqueMachineModeInShift: updated '{0}' to '{1}'", GetKey (PREVIOUS_SHIFT_ID), m_previousShiftId);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (PREVIOUS_SHIFT_ID), m_previousShiftId);
    }

    internal void ResetPreviousShiftId (int id)
    {
      m_previousShiftId = id;
    }

    internal DateTime? GetStartOfShift ()
    {
      return m_startShiftDateTime;
    }

    internal void UpdateStartOfShift (DateTime? startOfShift)
    {
      m_startShiftDateTime = startOfShift;
      log.InfoFormat ("AutoReasonUniqueMachineModeInShift: updated '{0}' to '{1}'", GetKey (START_OF_SHIFT), m_startShiftDateTime);
      if (m_startShiftDateTime != null && m_startShiftDateTime.HasValue) {
        ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (START_OF_SHIFT), m_startShiftDateTime.Value);
      }
      else {
        ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Remove (this.Machine, GetKey (START_OF_SHIFT));
      }
    }

    internal void ResetStartOfShift (DateTime? startOfShift)
    {
      m_startShiftDateTime = startOfShift;
    }

  }
}
