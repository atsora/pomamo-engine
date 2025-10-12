// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lemoine.Plugin.CncValueTime
{
  public class BeforeEndCncValue
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (BeforeEndCncValue).FullName);

    static readonly string TIMEOUT_KEY = "CncValueTime.BeforeEndCncValue.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string CACHE_TIME_OUT_KEY = "CncValueTime.BeforeEndCncValue.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (3);

    Configuration m_configuration;
    IMachineModule m_machineModule;
    IField m_field;
    string m_lambdaCondition;
    readonly ScriptOptions m_scriptOptions;
    Func<object, bool> m_lambda = null;
    Regex m_regexCondition = null;

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get {
        var suffix = "BeforeEnd";
        if (null == m_configuration) {
          if (!LoadConfiguration (out m_configuration)) {
            log.ErrorFormat ("Name.get: LoadConfiguration failed");
            return "";
          }
        }
        if ((null != m_configuration) && (null != m_configuration.NamePrefix)) {
          return m_configuration.NamePrefix + suffix;
        }
        else {
          return suffix;
        }
      }
    }

    public BeforeEndCncValue ()
      : base ()
    {
      m_scriptOptions = ScriptOptions.Default
        .AddReferences (new System.Reflection.Assembly[] {
          typeof (BeforeEndCncValue).Assembly,
          typeof (Convert).Assembly
        })
        .AddImports (new string[] { "System" });
    }

    bool IsMatch (ICncValue cncValue) =>
      (m_lambda?.Invoke (cncValue.Value) ?? true)
      && (m_regexCondition?.IsMatch (cncValue.Value.ToString ()) ?? true);

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      Debug.Assert (limit.ContainsElement (dateTime));

      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      Debug.Assert (limit.ContainsElement (dateTime));

      var range = new UtcDateTimeRange (limit
        .Intersects (hint)
        .Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime, "(]")));
      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("Get: dateTime={0} limit={1} hint={2} => No data", dateTime, limit, hint);
        }
        return this.CreateNoData ();
      }
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (range.Upper.Value.Equals (dateTime));
      Debug.Assert (!range.IsEmpty ());

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Lemoine.Plugin.CncValueTime.BeforeEndCncValue", TransactionLevel.ReadCommitted)) {
          ICncValue lastCncValue = null;

          var step = TimeSpan.FromHours (4);
          var cncValues = ModelDAOHelper.DAOFactory.CncValueDAO
            .FindOverlapsRangeDescending (m_machineModule, m_field, range, step);
          foreach (var cncValue in cncValues) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"GetValid: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (null == lastCncValue) { // first
              if (!cncValue.DateTimeRange.ContainsElement (dateTime)) {
                if (!IsValid (dateTime)) {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Get: no cnc value after {dateTime} => postpone the process");
                  }
                  return this.CreateWithHint (hint);
                }
              }
            }

            lastCncValue = cncValue;

            if (IsMatch (cncValue)) {
              if (range.ContainsElement (cncValue.End)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: condition met at {cncValue.End}");
                }
                return this.CreateFinal (cncValue.End);
              }
              else {
                Debug.Assert (dateTime <= cncValue.End); // Else hint was wrong
                Debug.Assert (cncValue.DateTimeRange.ContainsElement (dateTime));
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: the cnc value at {dateTime} meets the condition => return NotApplicable");
                }
                return this.CreateNotApplicable ();
              }
            }
          } // Loop on cncValues

          if (null == lastCncValue) {
            if (!IsValid (dateTime)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: no cnc value after {dateTime} => postpone the process");
              }
              return this.CreateWithHint (hint);
            }
          }

          return this.CreateNoData ();
        }
      }
    }

    bool IsValid (DateTime dateTime)
    {
      var firstCncValueAfter = ModelDAOHelper.DAOFactory.CncValueDAO
        .FindNext (m_machineModule, m_field, dateTime, 1)
        .FirstOrDefault ();
      return null != firstCncValueAfter;
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue || data.NoData) { // This is known...
        return TimeSpan.MaxValue;
      }

      if (m_configuration.CacheTimeOut.HasValue) {
        return m_configuration.CacheTimeOut.Value;
      }
      else {
        return Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (CACHE_TIME_OUT_KEY,
          CACHE_TIME_OUT_DEFAULT);
      }
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;
      LogManager.GetLogger (typeof (BeforeEndCncValue).FullName + "." + machine.Id);

      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: the configuration is not ok");
        return false;
      }

      // parameter = machine module id if not null
      // else the main machine module is considered
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (string.IsNullOrEmpty (parameter)) {
          var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (machine.Id);
          m_machineModule = monitoredMachine.MainMachineModule;
        }
        else { // null != m_machine
          int machineModuleId;
          if (!int.TryParse (parameter.Trim (), out machineModuleId)) {
            log.Error ($"Initialize: invalid machine module id {parameter}");
            return false;
          }
          m_machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (machineModuleId);
          if (null == m_machineModule) {
            log.Error ($"Initialize: unknown machine module id {machineModuleId}");
            return false;
          }
        }

        m_field = ModelDAOHelper.DAOFactory.FieldDAO
          .FindById (m_configuration.FieldId);
        if (null == m_field) {
          log.Error ($"Initialize: unknown field id {m_configuration.FieldId}");
          return false;
        }
      }

      if (string.IsNullOrEmpty (m_configuration.LambdaCondition) && string.IsNullOrEmpty (m_configuration.RegexCondition)) {
        log.Error ($"Initialize: no condition was defined");
        return false;
      }
      m_lambdaCondition = m_configuration.LambdaCondition;
      if (!string.IsNullOrEmpty (m_configuration.LambdaCondition)) {
        try {
          m_lambda = Task.Run (() => CSharpScript.EvaluateAsync<Func<object, bool>> (m_lambdaCondition, m_scriptOptions)).GetAwaiter ().GetResult ();
        }
        catch (Exception ex) {
          log.Error ($"Initialize: invalid expression {m_lambdaCondition}", ex);
          return false;
        }
      }
      if (!string.IsNullOrEmpty (m_configuration.RegexCondition)) {
        try {
          m_regexCondition = new Regex (m_configuration.RegexCondition);
        }
        catch (Exception ex) {
          log.Error ($"Initialize: invalid regex {m_configuration.RegexCondition}", ex);
          return false;
        }
      }

      return true;
    }

    public bool IsApplicable ()
    {
      return m_field.Active;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      return DynamicTimeApplicableStatus.Always;
    }
  }
}
