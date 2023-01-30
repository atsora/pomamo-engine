// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.CycleIsProduction
{
  public class NextProductionEnd
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    static readonly string CACHE_TIME_OUT_KEY = "CycleIsProduction.NextProductionStart.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "CycleIsProduction.NextProductionStart.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (3);

    ILog log = LogManager.GetLogger (typeof (NextProductionEnd).FullName);

    Configuration m_configuration;

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      log = LogManager.GetLogger ($"{typeof (NextProductionEnd).FullName}.{this.Machine?.Id}");
      if (string.IsNullOrEmpty (this.Name)) { // The configuration is loaded in Name.get
        return false;
      }
      Debug.Assert (null != m_configuration);
      return m_configuration.CheckMachineFilter (machine);
    }

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        var suffix = "NextProductionEnd";
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

    public bool IsApplicable ()
    {
      DateTime? cycleDetectionDateTime = GetCycleDetectionDateTime ();
      if (!cycleDetectionDateTime.HasValue) {
        return false;
      }

      var now = DateTime.UtcNow;
      TimeSpan applicableTimeSpan;
      if (m_configuration.ApplicableTimeSpan.HasValue) {
        applicableTimeSpan = m_configuration.ApplicableTimeSpan.Value;
      }
      else {
        applicableTimeSpan = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (APPLICABLE_TIME_SPAN_KEY,
          APPLICABLE_TIME_SPAN_DEFAULT);
      }
      var range = new UtcDateTimeRange (now.Subtract (applicableTimeSpan));
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("LastProductionEnd.IsApplicable")) {
          var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindFullOverlapsRangeDescending (this.Machine, range, 1);
          if (!cycles.Any ()) {
            return false;
          }
        }
      }

      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("CycleIsProduction.NextProductionEnd.IsApplicableAt")) {
          var cycleAt = ModelDAOHelper.DAOFactory.OperationCycleDAO
                      .FindAt (this.Machine, dateTime);
          if ((null != cycleAt) && cycleAt.End.HasValue && cycleAt.HasRealEnd ()) {
            return DynamicTimeApplicableStatus.YesAtDateTime;
          }
          var cycleDetectionDateTime = GetCycleDetectionDateTime ();
          if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
            if (null != cycleAt) {
              return DynamicTimeApplicableStatus.YesAtDateTime;
            }
            else {
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
          }
          else {
            return DynamicTimeApplicableStatus.Pending;
          }
        } // Transaction
      } // Sesssion
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("CycleIsProduction.NextProductionEnd")) {
          var cycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAt (this.Machine, dateTime);
          if (null == cycle) { // No cycle at dateTime
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Get: no cycle at {0}", dateTime);
            }
            var cycleDetectionDateTime = GetCycleDetectionDateTime ();
            if (cycleDetectionDateTime.HasValue) {
              if (Bound.Compare<DateTime> (dateTime, cycleDetectionDateTime.Value) < 0) {
                if (log.IsWarnEnabled) {
                  log.WarnFormat ("Get: no active cycle (and then no production) at {0} => return NotApplicable", dateTime);
                }
                return this.CreateNotApplicable ();
              }
              else {
                return this.CreatePending ();
              }
            }
            else { // !cycleDetectionDateTime.HasValue
              return this.CreatePending ();
            }
          }
          else { // null != cycle
            if (cycle.Full && cycle.End.HasValue) {
              bool extendCycle = Lemoine.Info.ConfigSet
                .LoadAndGet<bool> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendFullCycleWhenNewCycleEnd),
                     false);
              if (!extendCycle) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: cycle at {0} is already a full cycle and extend full cycle option is off => return its end {1}", dateTime, cycle.End.Value);
                }
                return this.CreateFinal (cycle.End.Value);
              }
            }

            var nextCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
              .GetFirstStrictlyAfter (this.Machine, dateTime);
            if (null != nextCycle) {
              if (!cycle.End.HasValue) {
                if (log.IsErrorEnabled) {
                  log.ErrorFormat ("Get: cycle at {0} has no end although there is a cycle after it => no data", dateTime);
                }
                return this.CreateNoData ();
              }
              else if (cycle.Full) { // && cycle.End.HasValue
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: full cycle at {0} with a next cycle => final", dateTime);
                }
                return this.CreateFinal (cycle.End.Value);
              }
              else {
                var operationDetectionDateTime = GetOperationDetectionDateTime ();
                if (operationDetectionDateTime.HasValue && nextCycle.DateTime <= operationDetectionDateTime.Value) {
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("Get: not a full cycle found at {0} with no future possible merge => NoData", dateTime);
                  }
                  return this.CreateNoData ();
                }
                else { // Possible later merge or estimated cycle end update
                  var newHint = new UtcDateTimeRange (nextCycle.DateTime);
                  newHint = new UtcDateTimeRange (hint.Intersects (newHint));
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("Get: operation detection date/time {0} before next cycle date/time {1} => with new hint {2}",
                      operationDetectionDateTime, nextCycle.DateTime, newHint);
                  }
                  return this.CreateWithHint (newHint);
                }
              }
            }
            else { // null == nextCycle
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: cycle at {0} is still active", dateTime);
              }
              return this.CreateWithHint (hint);
            }
          }
        }
      }
    }

    DateTime? GetCycleDetectionDateTime ()
    {
      var cycleDetectionStatusRequest = new Lemoine.Business.Operation
        .CycleDetectionStatus (this.Machine);
      var cycleDetectionStatus = Business.ServiceProvider
        .Get<DateTime?> (cycleDetectionStatusRequest);
      return cycleDetectionStatus;
    }

    DateTime? GetOperationDetectionDateTime ()
    {
      var operationDetectionStatusRequest = new Lemoine.Business.Operation
        .OperationDetectionStatus (this.Machine);
      var operationDetectionStatus = Business.ServiceProvider
        .Get<DateTime?> (operationDetectionStatusRequest);
      return operationDetectionStatus;
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue || data.NoData) { // This is known...
        return CacheTimeOut.Permanent.GetTimeSpan ();
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
  }
}
