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
using Lemoine.Extensions.AutoReason;
using Lemoine.Extensions.AutoReason.Action;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Lemoine.Plugin.AutoReasonCncValue
{
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    static readonly string NUMBER_CNC_VALUES_KEY = "AutoReason.CncValue.NumberCncValues";
    static readonly int NUMBER_CNC_VALUES_DEFAULT = 30;

    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);
    readonly ScriptOptions m_scriptOptions;
    Func<object, bool> m_lambda;
    string m_dynamicStart = null;
    string m_dynamicEnd = null;
    IList<IMachineModule> m_machineModules;
    IField m_field;
    Regex m_machineModulePrefixRegex = null;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension ()
      : base ("AutoReason.CncValue")
    {
      m_scriptOptions = ScriptOptions.Default
        .AddReferences (new System.Reflection.Assembly[] {
          typeof (AutoReasonExtension).Assembly,
          typeof (Convert).Assembly
        })
        .AddImports (new string[] { "System" });
    }

    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger (string.Format ("{0}.{1}", typeof (AutoReasonExtension).FullName, this.Machine.Id));

      try {
        m_lambda = Task.Run (() => CSharpScript.EvaluateAsync<Func<object, bool>> (configuration.LambdaCondition, m_scriptOptions)).Result;
      }
      catch (Exception ex) {
        log.Error ($"InitializeAdditionalConfigurations: invalid expression {configuration.LambdaCondition}", ex);
        return false;
      }

      m_dynamicStart = configuration.DynamicStart;
      m_dynamicEnd = configuration.DynamicEnd;
      if (string.IsNullOrEmpty (m_dynamicEnd)) {
        log.Error ("InitializeAdditionalConfigurations: invalid dynamic end");
        return false;
      }

      if (!string.IsNullOrEmpty (configuration.MachineModulePrefixRegex)) {
        try {
          m_machineModulePrefixRegex = new Regex (configuration.MachineModulePrefixRegex);
        }
        catch (ArgumentException ex) {
          log.Error ($"InitializeAdditionalConfigurations: regex {configuration.MachineModulePrefixRegex} for machine module prefix is not valid", ex);
          return false;
        }
      }

      var attachedMonitoredMachine = ModelDAOHelper.DAOFactory
        .MonitoredMachineDAO.FindByIdWithMachineModules (this.Machine.Id);
      if (null != m_machineModulePrefixRegex) {
        m_machineModules = attachedMonitoredMachine.MachineModules
          .Where (x => m_machineModulePrefixRegex.IsMatch (x.ConfigPrefix))
          .ToList ();
      }
      else {
      m_machineModules = attachedMonitoredMachine.MachineModules.ToList ();
      }
      if (!m_machineModules.Any ()) {
        log.ErrorFormat ("InitializeAdditionalConfigurations: no associated machine modules");
        return false;
      }

      int fieldId = configuration.FieldId;
      Debug.Assert (0 != fieldId);
      m_field = ModelDAOHelper.DAOFactory.FieldDAO
        .FindById (fieldId);
      if (null == m_field) {
        log.Error ($"InitializeAdditionalConfigurations: field id {fieldId} does not exist");
        return false;
      }

      return true;
    }

    /// <summary>
    /// May this auto-reason correspond to one of the possible extra auto-reasons of the specified reason slot
    /// 
    /// Because it is used only for some optimization purpose,
    /// return true, in doubt, in case it is not easy to determine it
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    /// <returns></returns>
    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    public override void RunOnce ()
    {
      var numberCncValues = Lemoine.Info.ConfigSet
        .LoadAndGet (NUMBER_CNC_VALUES_KEY, NUMBER_CNC_VALUES_DEFAULT);

      ClearRevision ();

      foreach (var machineModule in m_machineModules) {
        SetActive ();
        RunOnce (machineModule, numberCncValues);
      }
    }

    protected override void Check ()
    {
      log.Fatal ("Check: Check by machine module is used instead");
      throw new NotImplementedException ();
    }

    void RunOnce (IMachineModule machineModule, int numberCncValues)
    {
      try {
        Check (machineModule, numberCncValues);
        InitializeRevisionIfRequired ();
        this.ProcessPendingActions ();
      }
      catch (Exception ex) {
        log.Error ($"RunOnce: exception for machine module id {machineModule.Id}", ex);
        throw;
      }
    }

    void Check (IMachineModule machineModule, int numberCncValues)
    {
      var dateTime = GetDateTime (machineModule);
      var range = new UtcDateTimeRange (dateTime);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var cncValues = ModelDAOHelper.DAOFactory.CncValueDAO
          .FindFirstOverlapsRange (machineModule, m_field, range, numberCncValues, false);

        foreach (var cncValue in cncValues) {
          SetActive ();
          Debug.Assert (cncValue.DateTimeRange.Lower.HasValue);

          if (Bound.Compare<DateTime> (cncValue.DateTimeRange.Lower
            , dateTime) < 0) { // Already processed
            AddUpdateMachineModuleDateTimeDelayedAction (machineModule, cncValue.End.AddSeconds (1));
          }
          else {
            if (m_lambda.Invoke (cncValue.Value)) {
              DateTime reasonStart;
              if (!string.IsNullOrEmpty (m_dynamicStart)) {
                var dynamicStart = m_dynamicStart
                  .Replace ("MachineModuleId", machineModule.Id.ToString ())
                  .Replace ("FieldId", m_field.Id.ToString ());
                var start = Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime (dynamicStart, this.Machine, cncValue.Begin);
                if (start.NotApplicable) {
                  if (log.IsErrorEnabled) {
                    log.Error ($"Check: dynamic start {dynamicStart} returned NotApplicable at {cncValue.Begin} => skip it");
                  }
                  AddUpdateMachineModuleDateTimeDelayedAction (machineModule, cncValue.End.AddSeconds (1));
                  continue;
                }
                else if (start.NoData) {
                  if (log.IsWarnEnabled) {
                    log.Warn ($"Check: dynamic start {dynamicStart} returned NoData at {cncValue.Begin} => skip it");
                  }
                  AddUpdateMachineModuleDateTimeDelayedAction (machineModule, cncValue.End.AddSeconds (1));
                  continue;
                }
                else if (start.Final.HasValue) {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Check: dynamic start {dynamicStart} returned final {start.Final}");
                  }
                  reasonStart = start.Final.Value;
                }
                else {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Check: dynamic start {dynamicStart} returned {start} => postpone the process");
                  }
                  continue;
                }
              }
              else { // string.IsNullOrEmpty(m_dynamicStart)
                reasonStart = cncValue.Begin;
              }
              var reasonRange = new UtcDateTimeRange (reasonStart);
              var details = cncValue.Value.ToString ();
              var dynamicEnd = m_dynamicEnd
                .Replace ("MachineModuleId", machineModule.Id.ToString ())
                .Replace ("FieldId", m_field.Id.ToString ());
              AddReason (reasonRange, dynamicEnd, details);
            }
            AddUpdateMachineModuleDateTimeDelayedAction (machineModule, cncValue.End.AddSeconds (1));
          }
          // Because there may be cnc values with a null duration
          // Else there is a risk of a loop
        }

      }
    }

    void AddReason (UtcDateTimeRange range, string dynamic, string details)
    {
      var action = new ApplyReasonAction (this, range, dynamic, details);
      AddDelayedAction (action);
    }
  }
}
