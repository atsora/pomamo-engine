// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using Lemoine.Core.Log;
using System.Text.RegularExpressions;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonStopSequence
{
  /// <summary>
  /// Detect sequences of type "STOP"
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);
    string m_filter = "";

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.StopSequence")
    {
    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_filter = configuration.Filter;

      // Everything is ok
      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.StopSequence.Check")) {

          // Get the next sequence slot after the current date time, for all machine modules
          var nextSlots = new List<ISequenceSlot> ();
          foreach (IMachineModule mamo in Machine.MachineModules) {
            var nextSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO.FindFirstAfter (mamo, this.DateTime);
            if (nextSlot != null) {
              // Select only cncValues having its beginning and end specified
              if (nextSlot.DateTimeRange.Lower.HasValue && nextSlot.DateTimeRange.Upper.HasValue) {
                nextSlots.Add (nextSlot);
              }
            }
          }

          // Determine a DateTime for the next analyze: minimum of all cnc value start datetimes
          DateTime? nextDateTime = null;
          foreach (var nextSlot in nextSlots) {
            if (nextDateTime == null || nextSlot.DateTimeRange.Lower.Value < nextDateTime.Value) {
              nextDateTime = nextSlot.DateTimeRange.Lower.Value;
            }
          }

          if (nextDateTime != null) {
            if (nextDateTime < this.DateTime) // Should not append
{
              nextDateTime = this.DateTime;
            }

            // Browse all slots and apply a reason if:
            // * the sequence kind is "Stop"
            // * start date time is between DateTime and nextDateTime (including both limits of the range)
            foreach (var nextSlot in nextSlots) {
              SetActive ();
              // Sequence is stop?
              bool isStop = (nextSlot.Sequence != null && nextSlot.Sequence.Kind == SequenceKind.Stop);

              // In the right datetime range?
              if (nextSlot.DateTimeRange.Lower.Value >= this.DateTime && nextSlot.DateTimeRange.Lower.Value <= nextDateTime.Value && isStop) {
                // Apply a reason
                ProcessSequenceStop (nextSlot);
              }
            }

            // Update DateTime
            GoOn (nextDateTime.Value.AddSeconds (1));
          }
        }
      }
    }

    void GoOn (DateTime dateTime)
    {
      if (dateTime <= this.DateTime) {
        dateTime = this.DateTime.AddSeconds (1);
      }
      this.AddUpdateDateTimeDelayedAction (dateTime);
    }

    void ProcessSequenceStop (ISequenceSlot stopSlot)
    {
      Debug.Assert (stopSlot.DateTimeRange.Lower.HasValue);
      Debug.Assert (stopSlot.DateTimeRange.Upper.HasValue);
      Debug.Assert (stopSlot.Sequence != null);

      // Possibly skip the slot if the filter doesn't match the sequence name
      if (m_filter != "" && m_filter != null && !Regex.IsMatch (stopSlot.Sequence.Name, m_filter, RegexOptions.IgnoreCase)) {
        return;
      }

      var range = new UtcDateTimeRange (stopSlot.DateTimeRange.Lower);
      var dynamic = ",SequenceEnd(" + stopSlot.MachineModule.Id + ")";
      var details = stopSlot.Sequence.Name;
      AddReason (range, dynamic, details);
    }

    void AddReason (UtcDateTimeRange range, string dynamic, string details)
    {
      var action = new ApplyReasonAction (this, range, dynamic, details);
      AddDelayedAction (action);
    }
  }
}
