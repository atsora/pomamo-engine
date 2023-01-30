// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.Core.Cache;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Threading.Tasks;

namespace Lemoine.Plugin.CncValueTime
{
  public class AfterEndCncValue
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (AfterEndCncValue).FullName);

    static readonly string TIMEOUT_KEY = "CncValueTime.AfterEndCncValue.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string CACHE_TIME_OUT_KEY = "CncValueTime.AfterEndCncValue.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (3);

    Configuration m_configuration;
    IMachineModule m_machineModule;
    IField m_field;
    string m_lambdaCondition;
    readonly ScriptOptions m_scriptOptions;
    Func<object, bool> m_lambda;

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        var suffix = "AfterEnd";
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

    public AfterEndCncValue ()
      : base ()
    {
      m_scriptOptions = ScriptOptions.Default
        .AddReferences (new System.Reflection.Assembly[] {
          typeof (AfterEndCncValue).Assembly,
          typeof (Convert).Assembly
        })
        .AddImports (new string[] { "System" });
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      Debug.Assert (limit.ContainsElement (dateTime));

      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      Debug.Assert (limit.ContainsElement (dateTime));

      var range = new UtcDateTimeRange (limit
        .Intersects (hint)
        .Intersects (new UtcDateTimeRange (dateTime)));
      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("Get: dateTime={0} limit={1} hint={2} => No data", dateTime, limit, hint);
        }
        return this.CreateNoData ();
      }
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (range.Lower.HasValue);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Lemoine.Plugin.CncValueTime.AfterEndCncValue", TransactionLevel.ReadCommitted)) {
          ICncValue lastCncValue = null;
          ICncValue matchingCncValue = null;

          var step = TimeSpan.FromHours (4);
          var cncValues = ModelDAOHelper.DAOFactory.CncValueDAO
            .FindOverlapsRangeAscending (m_machineModule, m_field, range, step);
          foreach (var cncValue in cncValues) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"GetValid: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            lastCncValue = cncValue;

            if (null != matchingCncValue) {
              if (cncValue.Begin.Equals (matchingCncValue.End)
                && m_lambda.Invoke (cncValue.Value)) {
                if (range.ContainsElement (cncValue.End)) {
                  matchingCncValue = cncValue;
                  continue;
                }
                else {
                  return this.CreateNoData ();
                }
              }
              else {
                return this.CreateFinal (matchingCncValue.End);
              }
            }

            if (m_lambda.Invoke (cncValue.Value)) {
              if (range.ContainsElement (cncValue.End)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: condition met at {cncValue.End}");
                }
                matchingCncValue = cncValue;
                continue;
              }
              else { // Already after range
                Debug.Assert (range.Upper.HasValue);
                Debug.Assert (range.Upper.Value <= cncValue.End);
                Debug.Assert (!limit.ContainsElement (cncValue.End));
                return this.CreateNoData ();
              }
            }
          } // Loop on cncValues

          if (range.Upper.HasValue) {
            Debug.Assert (limit.Upper.HasValue);
            Debug.Assert (range.Upper.Value.Equals (limit.Upper.Value));
            var nextCncValue = GetNextCncValueAfter (range.Upper.Value);
            if (null == nextCncValue) { // More coming later ?
              if (null != matchingCncValue) {
                return CreateNewHintFromMatchingCncValue (hint, limit, matchingCncValue);
              }
              else if (null == lastCncValue) {
                return this.CreateWithHint (hint);
              }
              else if (range.Upper.Value <= lastCncValue.End) { // null != lastCncValue
                return this.CreateNoData ();
              }
              else {
                return CreateNewHint (hint, limit, lastCncValue);
              }
            }
            else { // null != nextCncValue
              if (null != matchingCncValue) {
                if (nextCncValue.Begin.Equals (matchingCncValue.End)
                  && m_lambda.Invoke (nextCncValue.Value)) {
                  Debug.Assert (!limit.ContainsElement (nextCncValue.End));
                  return this.CreateNoData ();
                }
                else {
                  return this.CreateFinal (matchingCncValue.End);
                }
              }
              else {
                return this.CreateNoData ();
              }
            }
          }
          else { // !range.Upper.HasValue
            if (null != matchingCncValue) {
              return CreateNewHintFromMatchingCncValue (hint, limit, matchingCncValue);
            }
            else {
              return CreateNewHint (hint, limit, lastCncValue);
            }
          }
        }
      }
    }

    IDynamicTimeResponse CreateNewHint (UtcDateTimeRange hint, UtcDateTimeRange limit, ICncValue lastCncValue)
    {
      if (null == lastCncValue) {
        return this.CreateWithHint (hint);
      }
      else {
        var newHint = new UtcDateTimeRange (hint
          .Intersects (new UtcDateTimeRange (lastCncValue.End)));
        if (!newHint.IsEmpty () && limit.Overlaps (newHint)) {
          return this.CreateWithHint (newHint);
        }
        else {
          return this.CreateNoData ();
        }
      }
    }

    IDynamicTimeResponse CreateNewHintFromMatchingCncValue (UtcDateTimeRange hint, UtcDateTimeRange limit, ICncValue matchingCncValue)
    {
      // Safer to consider .Begin here, else it will be necessary
      // at the next attempt to check if hint corresponds to
      // a matching cnc value or not

      Debug.Assert (null != matchingCncValue);

      var newHint = new UtcDateTimeRange (hint
        .Intersects (new UtcDateTimeRange (matchingCncValue.Begin)));
      if (!newHint.IsEmpty () && limit.Overlaps (newHint)) {
        return this.CreateWithHint (newHint);
      }
      else {
        return this.CreateNoData ();
      }
    }

    ICncValue GetNextCncValueAfter (DateTime dateTime)
    {
      return ModelDAOHelper.DAOFactory.CncValueDAO
        .FindNext (m_machineModule, m_field, dateTime.AddSeconds (1), 1)
        .FirstOrDefault ();
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
      LogManager.GetLogger (typeof (AfterEndCncValue).FullName + "." + machine.Id);

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

      m_lambdaCondition = m_configuration.LambdaCondition;
      if (string.IsNullOrEmpty (m_lambdaCondition)) {
        log.Error ($"Initialize: no lambda condition was defined");
        return false;
      }
      try {
        m_lambda = Task.Run (() => CSharpScript.EvaluateAsync<Func<object, bool>> (m_lambdaCondition, m_scriptOptions)).Result;
      }
      catch (Exception ex) {
        log.Error ($"Initialize: invalid expression {m_lambdaCondition}", ex);
        return false;
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
