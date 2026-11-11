// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
using Lemoine.Business;
using Lemoine.Business.Operation;

namespace Lemoine.Plugin.DynamicTimesCycle
{
  public class NextCycleSameOperation
    : Lemoine.Extensions.NotConfigurableExtension
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (NextCycleSameOperation).FullName);

    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "DynamicTimesCycle.NextCycleSameOperation.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (3);

    public IMachine Machine
    {
      get; private set;
    }

    public string Name => "NextCycleSameOperation";

    public bool IsApplicable ()
    {
      if (!IsCycleApplicable ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("IsApplicable: cycle is not applicable");
        }
        return false;
      }

      if (!IsOperationApplicable ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("IsApplicable: operation is not applicable");
        }
        return false;
      }

      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      // TODO: to optimize
      var dynamicTimeResult = Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      if (dynamicTimeResult.Timeout) {
        return DynamicTimeApplicableStatus.NoAtDateTime;
      }
      else if (dynamicTimeResult.NotApplicable) {
        return DynamicTimeApplicableStatus.NoAtDateTime;
      }
      else if (dynamicTimeResult.NoData) {
        log.Error ("IsApplicableAt: NoData with no limit");
        return DynamicTimeApplicableStatus.NoAtDateTime;
      }
      else if (dynamicTimeResult.Final.HasValue) {
        return DynamicTimeApplicableStatus.YesAtDateTime;
      }
      else {
        return DynamicTimeApplicableStatus.Pending;
      }
    }

    bool IsCycleApplicable ()
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
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.NextCycleSameOperation.IsCycleApplicable")) {
          var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindFullOverlapsRangeDescending (this.Machine, range, 1);
          if (!cycles.Any ()) {
            return false;
          }
        }
      }

      return true;
    }

    DateTime? GetCycleDetectionDateTime ()
    {
      var cycleDetectionStatusRequest = new Lemoine.Business.Operation
        .CycleDetectionStatus (this.Machine);
      var cycleDetectionStatus = Business.ServiceProvider
        .Get<DateTime?> (cycleDetectionStatusRequest);
      return cycleDetectionStatus;
    }

    bool IsOperationApplicable ()
    {
      DateTime? operationDetectionDateTime = GetOperationDetectionDateTime ();
      if (!operationDetectionDateTime.HasValue) {
        return false;
      }

      var now = DateTime.UtcNow;
      TimeSpan applicableTimeSpan = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (APPLICABLE_TIME_SPAN_KEY,
          APPLICABLE_TIME_SPAN_DEFAULT);
      var lower = now.Subtract (applicableTimeSpan);
      var range = new UtcDateTimeRange (lower);
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.NextCycleSameOperation.IsOperationApplicable")) {
          var exists = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .ExistsInRange (this.Machine, range);
          if (!exists) {
            if (log.IsDebugEnabled) {
              log.Debug ($"IsOperationApplicable: no operation slot in {range}");
            }
            return false;
          }
          var lastOperationNotNull = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetLastOperationNotNull (this.Machine);
          if (null == lastOperationNotNull) {
            if (log.IsDebugEnabled) {
              log.Debug ("IsOperationApplicable: no operation slot with operation not null");
            }
            return false;
          }
          if (Bound.Compare<DateTime> (lastOperationNotNull.DateTimeRange.Upper, lower) < 0) {
            if (log.IsDebugEnabled) {
              log.Debug ($"IsOperationApplicable: last operation not null before {lower}");
            }
            return false;
          }
        }
      }

      return true;
    }

    DateTime? GetOperationDetectionDateTime ()
    {
      var operationDetectionStatusRequest = new Lemoine.Business.Operation
        .OperationDetectionStatus (this.Machine);
      var operationDetectionStatus = Business.ServiceProvider
        .Get<DateTime?> (operationDetectionStatusRequest);
      return operationDetectionStatus;
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // 1. Get the next cycle
        IOperationCycle nextCycle;
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.NextCycleSameOperation.NextCycle")) {
          nextCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .GetFirstStrictlyAfter (this.Machine, dateTime);
          if (null == nextCycle) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: no cycle after {dateTime} => pending");
            }
            return this.CreatePending ();
          }
        }
        if (!nextCycle.Begin.HasValue) {
          if (log.IsWarnEnabled) {
            log.Warn ($"Get: nextCycle {nextCycle.Id} has no start => cancel");
          }
          return this.CreateNotApplicable ();
        }
        if (nextCycle.HasRealBegin ()) {
          if (nextCycle.Begin.Value.Equals (dateTime)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: nextCycle real start {dateTime} is dateTime");
            }
            return this.CreateFinal (dateTime);
          }
          if (nextCycle.Begin.Value < dateTime) {
            if (log.IsWarnEnabled) {
              log.Warn ($"Get: {dateTime} is inside the cycle {nextCycle.Begin}-{nextCycle.End} => return not applicable");
            }
            return this.CreateNotApplicable ();
          }
          if (!limit.ContainsElement (nextCycle.Begin.Value)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: next cycle after limit {limit} => no data");
            }
            return this.CreateNoData ();
          }
        }

        // 2. Operation detection status
        var operationDetectionDateTime = ServiceProvider
          .Get<DateTime?> (new OperationDetectionStatus (this.Machine));
        if (!operationDetectionDateTime.HasValue) {
          if (log.IsDebugEnabled) {
            log.Debug ("Get: no operation detection date/time => pending");
          }
          return this.CreatePending ();
        }
        if (nextCycle.HasRealBegin ()) {
          if (operationDetectionDateTime.Value <= nextCycle.Begin.Value) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: operation detection date/time {operationDetectionDateTime} before next cycle begin {nextCycle.Begin} => pending");
            }
            return this.CreatePending ();
          }
        }
        else { // Estimated start
          if (!nextCycle.HasRealEnd ()) {
            log.Fatal ($"Get: nextCycle {nextCycle.Id} with an estimated start and an estimated end");
            Debug.Assert (false);
            return this.CreateNotApplicable ();
          }
          Debug.Assert (nextCycle.End.HasValue);
          if (operationDetectionDateTime.Value <= nextCycle.End.Value) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: operation detection date/time {operationDetectionDateTime} before next cycle end {nextCycle.End} (estimated start) => pending");
            }
            // Because of a possible merge later
            return this.CreatePending ();
          }

          if (nextCycle.Begin.Value.Equals (dateTime)) {
            Debug.Assert (!nextCycle.HasRealBegin ());
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: nextCycle estimated start {dateTime} is dateTime");
            }
            return this.CreateFinal (dateTime);
          }
          if (!limit.ContainsElement (nextCycle.Begin.Value)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: next cycle after limit {limit} => no data");
            }
            return this.CreateNoData ();
          }
        } // Estimated start

        // 3. Operation slots
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.NextCycleSameOperation.OperationSlots")) {
          var range = new UtcDateTimeRange (dateTime, nextCycle.Begin.Value);
          Debug.Assert (!range.IsEmpty ());
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRange (this.Machine, range);
          var operationCounts = operationSlots
            .Where (s => null != s.Operation)
            .Select (s => s.Operation)
            .Distinct ()
            .Count ();
          if (1 == operationCounts) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: one operation in range {range} => return {nextCycle.Begin}");
            }
            return this.CreateFinal (nextCycle.Begin.Value);
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: {operationCounts} operations in range {range} => cancel");
            }
            return this.CreateNotApplicable ();
          }
        }
      }
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

      log = LogManager.GetLogger (typeof (NextCycleSameOperation).FullName + "." + machine.Id);

      return true;
    }
  }
}
