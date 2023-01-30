// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.Action;
using System.Text.RegularExpressions;

namespace Lemoine.Plugin.AutoReasonStopDuringMachiningSequence
{
  /// <summary>
  /// Detect stop during machining sequences
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    // Configuration
    string m_sequenceNamePattern;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.StopDuringMachiningSequence")
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

      m_sequenceNamePattern = configuration.SequenceNameRegex;

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
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.StopDuringMachiningSequence.Check")) {

          // Get the next sequence slot after the current date time, for all machine modules
          var nextSlots = new List<ISequenceSlot> ();
          foreach (IMachineModule mamo in Machine.MachineModules) {
            SetActive ();
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
            // * the sequence name match the regex pattern TODO
            // * the sequence kind is machining
            // * start date time is between DateTime and nextDateTime (including both limits of the range)
            foreach (var nextSlot in nextSlots) {
              SetActive ();
              // Don't process if there is no sequence
              if (nextSlot.Sequence == null) {
                continue;
              }

              // Or if it's not machining
              if (nextSlot.Sequence.Kind != SequenceKind.Machining) {
                continue;
              }

              // Or if the name doesn't match the pattern
              if (!string.IsNullOrEmpty (m_sequenceNamePattern) && !Regex.IsMatch (nextSlot.Sequence.Name, m_sequenceNamePattern)) {
                continue;
              }

              // In the right datetime range?
              if (nextSlot.DateTimeRange.Lower.Value < this.DateTime || nextSlot.DateTimeRange.Lower.Value > nextDateTime.Value) {
                continue;
              }

              // Everything is ok: a reason is now applied
              ProcessMachiningSequence (nextSlot);              
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
      AddUpdateDateTimeDelayedAction (dateTime);
    }

    void ProcessMachiningSequence (ISequenceSlot stopSlot)
    {
      Debug.Assert (stopSlot.DateTimeRange.Lower.HasValue);
      Debug.Assert (stopSlot.DateTimeRange.Upper.HasValue);
      Debug.Assert (stopSlot.Sequence != null);

      var range = new UtcDateTimeRange (stopSlot.DateTimeRange.Lower.Value);
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
