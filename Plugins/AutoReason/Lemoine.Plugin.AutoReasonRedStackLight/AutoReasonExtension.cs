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

namespace Lemoine.Plugin.AutoReasonRedStackLight
{
  /// <summary>
  /// Detect stops between cycles and apply an autoreason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);
    bool m_writeAllAlarms;
    bool m_redOnly;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.RedStackLight")
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

      // Alarm configuration
      m_writeAllAlarms = configuration.WriteAllAlarms;
      m_redOnly = configuration.RedOnly;

      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.RedStacklight.Check")) {

          // Get the next stacklight slots after the current date time, for all machine modules
          // In the same time, compute the datetime before which we can read all alarms
          // and the minimum datetime for all 
          var stackLightField = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.StackLight);
          var nextCncValues = new List<ICncValue> ();
          DateTime? lastAlarmTimeStamp = null;
          DateTime? nextDateTime = null;

          foreach (IMachineModule mamo in this.Machine.MachineModules) {
            SetActive ();
            var cncValuesTmp = ModelDAOHelper.DAOFactory.CncValueDAO.FindNext (mamo, stackLightField, this.DateTime, 1);
            if (cncValuesTmp != null && cncValuesTmp.Count == 1) {
              // Select only cncValues having its beginning and end specified + a value as an integer
              var cncValue = cncValuesTmp[0];
              if (cncValue.DateTimeRange.Lower.HasValue && cncValue.DateTimeRange.Upper.HasValue && cncValue.Int.HasValue) {
                nextCncValues.Add (cncValue);

                // Minimum date time of all cnc values
                DateTime dateTimeTmp = cncValue.DateTimeRange.Lower.Value;
                if (nextDateTime == null || dateTimeTmp < nextDateTime.Value) {
                  nextDateTime = dateTimeTmp;
                }
              }
            }

            var acquisitionState = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (mamo, AcquisitionStateKey.Alarms);
            if (acquisitionState != null) {
              // Possibly update lastAlarmTimeStamp
              if (!lastAlarmTimeStamp.HasValue || lastAlarmTimeStamp.Value > acquisitionState.DateTime) {
                lastAlarmTimeStamp = acquisitionState.DateTime;
              }
            }
          }

          // Do we have a minimum datetime?
          if (!nextDateTime.HasValue) {
            return; // Nothing more to do in this case
          }

          // If the lastAlarmTimeStamp is too old (7 days old), we ignore it
          if (lastAlarmTimeStamp.HasValue && lastAlarmTimeStamp.Value < nextDateTime.Value.AddDays(-7)) {
            lastAlarmTimeStamp = null;
          }

          // nextDateTime shouldn't be less than the current datetime
          if (lastAlarmTimeStamp < this.DateTime) {
            nextDateTime = this.DateTime;
          }

          // If lastAlarmTimeStamp is not null and if lastAlarmTimeStamp < lastAlarmTimeStamp, we wait
          if (lastAlarmTimeStamp.HasValue && lastAlarmTimeStamp < nextDateTime.Value) {
            return;
          }

          // Browse all nextCncValues and apply a reason if:
          // * the light is red
          // * start date time is between DateTime and nextDateTime (including both limits of the range)
          foreach (var cncValue in nextCncValues) {
            SetActive ();
            // Stacklight is red?
            var stackLight = (StackLight)cncValue.Int.Value;
            bool triggerAutoReason;
            if (m_redOnly) {
              triggerAutoReason = stackLight.Equals (StackLight.RedOn) || stackLight.Equals (StackLight.RedFlashing);
            }
            else {
              triggerAutoReason = stackLight.IsOnOrFlashingIfAcquired (StackLightColor.Red);
            }

            // In the right datetime range?
            if (triggerAutoReason && cncValue.DateTimeRange.Lower.Value >= this.DateTime && cncValue.DateTimeRange.Lower.Value <= nextDateTime.Value) {
              // Apply a reason
              ProcessRedStackLight (cncValue, lastAlarmTimeStamp.HasValue);
            }
          }

          // Update DateTime
          GoOn (nextDateTime.Value.AddSeconds (1));
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

    void ProcessRedStackLight (ICncValue redStackLightValue, bool alarmAcquisitionOk)
    {
      Debug.Assert (redStackLightValue.DateTimeRange.Lower.HasValue);
      
      var range = new UtcDateTimeRange (redStackLightValue.DateTimeRange.Lower);

      // Add the list of alarms found at the beginning of the stacklight
      var alarms = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindAtWithSeverity (redStackLightValue.MachineModule, redStackLightValue.DateTimeRange.Lower.Value);
      var details = "";

      if (alarmAcquisitionOk) {
        if (alarms.Count == 0) {
          details = "no alarms";
        }
        else if (m_writeAllAlarms) {
          // First write focused alarms in details
          for (int i = alarms.Count - 1; i >= 0; i--) {
            var alarm = alarms[i];
            if (alarm.Severity != null && alarm.Severity.Focus == true) {
              alarms.RemoveAt (i);
              if (details.Length > 0) {
                details += "\n";
              }

              details += alarm.Number + ": '" + alarm.Message + "' (focused alarm)";
            }
          }

          // Then write alarms with an unknown focused state
          for (int i = alarms.Count - 1; i >= 0; i--) {
            var alarm = alarms[i];
            if (alarm.Severity == null || alarm.Severity.Focus == null) {
              alarms.RemoveAt (i);
              if (details.Length > 0) {
                details += "\n";
              }

              details += alarm.Number + ": '" + alarm.Message + "' (unknown focused state)";
            }
          }

          // Finally write ignored alarms
          for (int i = alarms.Count - 1; i >= 0; i--) {
            var alarm = alarms[i];
            if (details.Length > 0) {
              details += "\n";
            }

            details += alarm.Number + ": '" + alarm.Message + "' (ignored alarm)";
          }
        } else {
          // First write focused alarms in details
          bool withFocused = false;
          for (int i = alarms.Count - 1; i >= 0; i--) {
            var alarm = alarms[i];
            if (alarm.Severity != null && alarm.Severity.Focus == true) {
              withFocused = true;
              alarms.RemoveAt (i);
              if (details.Length > 0) {
                details += "\n";
              }

              details += alarm.Number + ": '" + alarm.Message + "' (focused alarm)";
            }
          }

          // If nothing is focused, write alarms with an unknown focused state
          if (!withFocused) {
            bool withUnknown = false;
            for (int i = alarms.Count - 1; i >= 0; i--) {
              var alarm = alarms[i];
              if (alarm.Severity == null || alarm.Severity.Focus == null) {
                withUnknown = true;
                alarms.RemoveAt (i);
                if (details.Length > 0) {
                  details += "\n";
                }

                details += alarm.Number + ": '" + alarm.Message + "' (unknown focused state)";
              }
            }

            if (!withUnknown) {
              // Everything should be ignored
              details = "only ignored alarms";
            }
          }
        }
      }

      AddReason (range, details);
    }

    void AddReason (UtcDateTimeRange range, string details)
    {
      var action = new ApplyReasonAction (this, range, ",NextProductionStart", details);
      AddDelayedAction (action);
    }
  }
}
