// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.DynamicTimesCycle
{
  public class CycleEnd
    : Lemoine.Extensions.NotConfigurableExtension
    , IDynamicTimeExtension
  {
    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "DynamicTimesCycle.CycleEnd.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (7);

    ILog log = LogManager.GetLogger (typeof (CycleEnd).FullName);

    public IMachine Machine { get; private set; }

    public string Name
    {
      get {
        return "CycleEnd";
      }
    }

    public bool IsApplicable ()
    {
      DateTime? cycleDetectionDateTime = GetCycleDetectionDateTime ();
      if (!cycleDetectionDateTime.HasValue) {
        return false;
      }

      var now = DateTime.UtcNow;
      TimeSpan applicableTimeSpan = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (APPLICABLE_TIME_SPAN_KEY,
        APPLICABLE_TIME_SPAN_DEFAULT);
      var range = new UtcDateTimeRange (now.Subtract (applicableTimeSpan));
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.CycleEnd.IsApplicable")) {
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
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.CycleEnd.IsApplicableAt")) {
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
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: dateTime={dateTime} hint={hint} limit={limit}");
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.CycleEnd")) {
          var cycleAt = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAt (this.Machine, dateTime);
          if (null != cycleAt) {
            if (cycleAt.End.HasValue && cycleAt.HasRealEnd ()) {
              bool extendCycle = Lemoine.Info.ConfigSet
                .LoadAndGet<bool> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendFullCycleWhenNewCycleEnd),
                     false);
              if (!extendCycle) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: cycle with real end found at {0} and extend full cycle option is off", dateTime);
                }
                return this.CreateFinal (cycleAt.End.Value);
              }
            }
            var cycleAfter = ModelDAOHelper.DAOFactory.OperationCycleDAO
              .GetFirstStrictlyAfter (this.Machine, cycleAt.DateTime);
            if (null != cycleAfter) {
              if (!cycleAt.End.HasValue) {
                if (log.IsErrorEnabled) {
                  log.ErrorFormat ("Get: cycle at {0} has no end although there is a cycle after it", dateTime);
                }
                return this.CreateNoData ();
              }
              else if (cycleAt.HasRealEnd ()) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: cycle with real end found at {0} and there is a cycle after it", dateTime);
                }
                return this.CreateFinal (cycleAt.End.Value);
              }

              var operationDetectionDateTime = GetOperationDetectionDateTime ();
              if (operationDetectionDateTime.HasValue && cycleAfter.DateTime <= operationDetectionDateTime.Value) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: cycle with estimated end found at {0} with a cycle after it", dateTime);
                }
                return this.CreateFinal (cycleAt.End.Value);
              }
              else { // Possible later merge or estimated cycle end update
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: operation detection date/time {0} before next cycle date/time {1} => with hint {2}",
                    operationDetectionDateTime, cycleAfter.DateTime, hint);
                }
                return this.CreateWithHint (hint);
              }
            }
            else { // null == cycleAfter
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: active cycle at {0}",
                  dateTime);
              }
              return this.CreateWithHint (hint);
            }
          }
          else { // null == cycleAt
            var cycleDetectionDateTime = GetCycleDetectionDateTime ();
            if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
              if (log.IsErrorEnabled) {
                log.ErrorFormat ("Get: there is no cycle at {0}, detection date/time is {1} => not applicable",
                  dateTime, cycleDetectionDateTime);
              }
              return this.CreateNotApplicable ();
            }
            else { // cycle detection date/time in the past
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: detection date/time in the past {0} before {1}",
                  cycleDetectionDateTime, dateTime);
              }
              return this.CreatePending ();
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
      if (data.Final.HasValue || data.NoData) {
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }
      else {
        return CacheTimeOut.CurrentLong.GetTimeSpan ();
      }
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      log = LogManager.GetLogger (typeof (CycleEnd).FullName + "." + machine.Id);

      return true;
    }
  }
}
