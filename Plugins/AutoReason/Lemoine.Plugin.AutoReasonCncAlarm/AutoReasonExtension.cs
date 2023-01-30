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
using System.Linq;
using Lemoine.Extensions.AutoReason;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonCncAlarm
{
  /// <summary>
  /// Detect stops between cycles and apply an autoreason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    bool m_focusedOnly = false;
    IMachineModule m_machineModuleFilter = null;
    ICncAlarmSeverity m_cncAlarmSeverity = null;
    Regex m_messageRegex = null;
    Regex m_excludeRegex = null;
    string m_dynamicEnd;
    string m_reasonDetailsPrefix;
    IList<IMachineModule> m_machineModules;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base ("AutoReason.CncAlarm")
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

      m_focusedOnly = configuration.FocusedOnly;

      if (0 != configuration.MachineModuleId) {
        m_machineModuleFilter = ModelDAOHelper.DAOFactory.MachineModuleDAO
          .FindById (configuration.MachineModuleId);
        if (null == m_machineModuleFilter) {
          log.Error ($"InitializeAdditionalConfigurations: machine module id {configuration.MachineModuleId} does not exist");
          return false;
        }
      }

      if (0 != configuration.CncAlarmSeverityId) {
        m_cncAlarmSeverity = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO
          .FindById (configuration.CncAlarmSeverityId);
        if (null == m_cncAlarmSeverity) {
          log.Error ($"InitializeAdditionalConfigurations: cnc alarm severity id {configuration.CncAlarmSeverityId} does not exist");
          return false;
        }
      }

      if (string.IsNullOrEmpty (configuration.MessageRegex)) {
        m_messageRegex = null;
      }
      else {
        try {
          m_messageRegex = new Regex (configuration.MessageRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
        catch (Exception ex) {
          log.Error ($"InitializeAdditionalConfigurations: regex {configuration.MessageRegex} is not valid", ex);
          return false;
        }
      }

      if (string.IsNullOrEmpty (configuration.ExcludeRegex)) {
        m_excludeRegex = null;
      }
      else {
        try {
          m_excludeRegex = new Regex (configuration.ExcludeRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
        catch (Exception ex) {
          log.Error ($"InitializeAdditionalConfigurations: exclude regex {configuration.ExcludeRegex} is not valid", ex);
          return false;
        }
      }

      m_dynamicEnd = configuration.DynamicEnd;
      m_reasonDetailsPrefix = configuration.DetailsPrefix;

      var attachedMonitoredMachine = ModelDAOHelper.DAOFactory
        .MonitoredMachineDAO.FindByIdWithMachineModules (this.Machine.Id);
      m_machineModules = attachedMonitoredMachine.MachineModules.ToList ();
      if (!m_machineModules.Any ()) {
        log.Error ("InitializeAdditionalConfigurations: no associated machine modules");
        return false;
      }

      // Everything is ok
      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    public override void RunOnce ()
    {
      ClearRevision ();

      foreach (var machineModule in m_machineModules) {
        SetActive ();
        RunOnce (machineModule);
      }
    }

    protected override void Check ()
    {
      log.Fatal ("Check: Check by machine module is used instead");
      throw new NotImplementedException ();
    }

    void RunOnce (IMachineModule machineModule)
    {
      Check (machineModule);
      InitializeRevisionIfRequired ();
      this.ProcessPendingActions ();
    }

    void Check (IMachineModule machineModule)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dateTime = GetDateTime (machineModule);

        var alarmAcquisitionDateTime = GetAlarmAcquisitionDateTime (machineModule);
        if (!alarmAcquisitionDateTime.HasValue) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"Check: no alarm acquisition date/time for machine module {machineModule.Id}");
          }
          return;
        }

        var period = new UtcDateTimeRange (dateTime, alarmAcquisitionDateTime.Value, "[]");
        if (period.IsEmpty ()) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"Check: empty period {this.DateTime}-{alarmAcquisitionDateTime}");
          }
          return;
        }

        using (var transaction = session.BeginReadOnlyTransaction ($"AutoReason.CncAlarm.Check.{this.Machine.Id}")) {
          var alarms = ModelDAOHelper.DAOFactory.CncAlarmDAO
            .FindWithBeginningInRangeWithSeverity (machineModule, period)
            .Where (a => IsMatch (a));
          if (GetLogger ().IsDebugEnabled) { 
            log.Debug ($"Check: got {alarms.Count ()} alarms");
          }
          foreach (var alarm in alarms) {
            SetActive ();
            ProcessAlarm (alarm);
          }
          SetActive ();

          AddUpdateMachineModuleDateTimeDelayedAction (machineModule, alarmAcquisitionDateTime.Value.AddSeconds (1));
        }
      }
    }

    DateTime? GetAlarmAcquisitionDateTime (IMachineModule machineModule)
    {
      var acquisitionState = ModelDAOHelper.DAOFactory.AcquisitionStateDAO
        .GetAcquisitionState (machineModule, AcquisitionStateKey.Alarms);
      // With a recent compiler, you can use: acquisitionState?.DateTime
      return (null != acquisitionState) ? acquisitionState.DateTime : (DateTime?)null;
    }

    bool IsMatch (ICncAlarm cncAlarm)
    {
      if (m_focusedOnly && !IsFocused (cncAlarm)) {
        return false;
      }

      if ((null != m_machineModuleFilter) && (cncAlarm.MachineModule.Id != m_machineModuleFilter.Id)) {
        return false;
      }

      if (null != m_cncAlarmSeverity) {
        if (cncAlarm.Severity is null) {
          if (GetLogger ().IsWarnEnabled) {
            GetLogger ().Warn ($"IsMatch: cnc alarm {cncAlarm.Id} with no severity");
          }
          return false;
        }
        else if (cncAlarm.Severity.Id != m_cncAlarmSeverity.Id) {
          return false;
        }
      }

      if (null != m_messageRegex) {
        if (!m_messageRegex.IsMatch (cncAlarm.Message)) {
          return false;
        }
      }

      if (null != m_excludeRegex) {
        if (m_excludeRegex.IsMatch (cncAlarm.Message)) {
          return false;
        }
      }

      return true;
    }

    bool IsFocused (ICncAlarm cncAlarm)
    {
      return (null != cncAlarm.Severity)
        && cncAlarm.Severity.Focus.HasValue
        && cncAlarm.Severity.Focus.Value;
    }

    void ProcessAlarm (ICncAlarm alarm)
    {
      Debug.Assert (alarm.DateTimeRange.Lower.HasValue);

      var range = new UtcDateTimeRange (alarm.DateTimeRange.Lower.Value);
      var details = alarm.Number + ": " + alarm.Message;
      if (!string.IsNullOrEmpty (m_reasonDetailsPrefix)) {
        details = m_reasonDetailsPrefix + details;
      }
      string dynamic;
      if (string.IsNullOrEmpty (m_dynamicEnd)) {
        log.Warn ("ProcessAlarm: no dynamic end was set, consider NextProductionStart instead");
        dynamic = ",NextProductionStart";
      }
      else {
        dynamic = "," + m_dynamicEnd;
      }
      var action = new ApplyReasonAction (this, range, dynamic, details);
      AddDelayedAction (action);
    }
  }
}
