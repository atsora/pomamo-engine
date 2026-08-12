// Copyright (C) 2026 Atsora Solutions

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
  /// <summary>
  /// Dynamic time: PreviousCycleEnd
  /// 
  /// End date/time of the previous cycle.
  /// There must be no active cycle at DateTime
  /// </summary>
  public class PreviousCycleEnd
    : Lemoine.Extensions.NotConfigurableExtension
    , IDynamicTimeExtension
  {
    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "DynamicTimesCycle.PrevCycleEnd.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (7);

    ILog log = LogManager.GetLogger (typeof (CycleEnd).FullName);

    public IMachine Machine { get; private set; }

    public string Name => "PreviousCycleEnd";

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
          if (null != cycleAt) {
            if (cycleAt.End.HasValue && cycleAt.HasRealEnd ()) {
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
            var cycleDetectionDateTime = GetCycleDetectionDateTime ();
            if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
            else {
              return DynamicTimeApplicableStatus.Pending;
            }
          }
          else { // No cycle at 
            var cycleDetectionDateTime = GetCycleDetectionDateTime ();
            if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
              return DynamicTimeApplicableStatus.YesAtDateTime;
            }
            else {
              return DynamicTimeApplicableStatus.Pending;
            }
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
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesCycle.PreviousCycleEnd")) {
          var cycleAt = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAt (this.Machine, dateTime);
          if (null != cycleAt) {
            if (cycleAt.End.HasValue && cycleAt.HasRealEnd ()) {
              return this.CreateNotApplicable ();
            }
            var cycleDetectionDateTime = GetCycleDetectionDateTime ();
            if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
              return this.CreateNotApplicable ();
            }
            else if (cycleDetectionDateTime.HasValue) {
              var newHint = new UtcDateTimeRange (cycleDetectionDateTime.Value);
              return this.CreateWithHint (new UtcDateTimeRange (hint.Intersects (newHint)));
            }
            else {
              log.Error ($"Get: cycle at {cycleAt.Begin} but no cycleDetectionDateTime");
              var newHint = new UtcDateTimeRange (cycleAt.Begin.Value);
              return this.CreateWithHint (new UtcDateTimeRange (hint.Intersects (newHint)));
            }
          }
          else { // no current cycle
            var previousCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
              .GetFirstStrictlyBefore (this.Machine, dateTime);
            if (null != previousCycle) {
              if (previousCycle.End.HasValue) {
                // Check the operationcycle date/time is after firstOperationCycleEnd first
                var cycleDetectionDateTime = GetCycleDetectionDateTime ();
                if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
                  return this.CreateFinal (previousCycle.End.Value);
                }
                else {
                  var newHint = new UtcDateTimeRange (previousCycle.End.Value);
                  return this.CreateWithHint (new UtcDateTimeRange (hint.Intersects (newHint)));
                }
              }
              else { // No end date/time for the previous cycle
                var cycleDetectionDateTime = GetCycleDetectionDateTime ();
                if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
                  log.Fatal ($"Get: valid previous cycle with no end date/time and no current cycle");
                  return this.CreateNotApplicable ();
                }
                else if (cycleDetectionDateTime.HasValue) {
                  var newHint = new UtcDateTimeRange (cycleDetectionDateTime.Value);
                  return this.CreateWithHint (new UtcDateTimeRange (hint.Intersects (newHint)));
                }
                else {
                  log.Error ($"Get: previousCycle at {previousCycle.DateTime} but no cycleDetectionDateTime");
                  var newHint = new UtcDateTimeRange (previousCycle.Begin.Value);
                  return this.CreateWithHint (new UtcDateTimeRange (hint.Intersects (newHint)));
                }
              }
            }
            else { // No previousCycle
              var cycleDetectionDateTime = GetCycleDetectionDateTime ();
              if (cycleDetectionDateTime.HasValue && dateTime <= cycleDetectionDateTime.Value) {
                return this.CreateNotApplicable ();
              }
              else {
                return this.CreatePending ();
              }
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

      log = LogManager.GetLogger (typeof (PreviousCycleEnd).FullName + "." + machine.Id);

      return true;
    }
  }
}
