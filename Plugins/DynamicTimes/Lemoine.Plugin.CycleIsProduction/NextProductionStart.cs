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
  public class NextProductionStart
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    static readonly string CACHE_TIME_OUT_KEY = "CycleIsProduction.NextProductionStart.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "CycleIsProduction.NextProductionStart.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (3);

    ILog log = LogManager.GetLogger (typeof (NextProductionStart).FullName);

    Configuration m_configuration;

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      log = LogManager.GetLogger ($"{typeof (NextProductionStart).FullName}.{this.Machine?.Id}");
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
        var suffix = "NextProductionStart";
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
        using (var transaction = session.BeginReadOnlyTransaction ("NextProductionStart.IsApplicable")) {
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
        var cycleDetectionDateTime = GetCycleDetectionDateTime ();
        if (!cycleDetectionDateTime.HasValue
          || Bound.Compare<DateTime> (cycleDetectionDateTime.Value, dateTime) <= 0) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("IsApplicableAt: cycle detection date/time {0} before date/time {1} => pending", cycleDetectionDateTime, dateTime);
          }
          // This is not possible to know if there will be a cycle at date/time
          // and to know if it will be NotApplicable or not
          return DynamicTimeApplicableStatus.Pending;
        }

        var cycleAt = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAt (this.Machine, dateTime);
        if (null != cycleAt) {
          return DynamicTimeApplicableStatus.NoAtDateTime;
        }
        else {
          return DynamicTimeApplicableStatus.YesAtDateTime;
        }
      }
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var range = new UtcDateTimeRange (hint
        .Intersects (new UtcDateTimeRange (dateTime)));
      var effectiveAfter = range.Lower.Value;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var cycleDetectionDateTime = GetCycleDetectionDateTime ();
        if (!cycleDetectionDateTime.HasValue
          || Bound.Compare<DateTime> (cycleDetectionDateTime.Value, dateTime) <= 0) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Get: cycle detection date/time {0} before date/time {1} => pending", cycleDetectionDateTime, dateTime);
          }
          // This is not possible to know if there will be a cycle at date/time
          // and to know if it will be NotApplicable or not
          return this.CreatePending ();
        }

        using (var transaction = session.BeginReadOnlyTransaction ("CycleIsProduction.NextProductionStart.CycleAt")) {
          var cycleAt = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAt (this.Machine, dateTime);
          if (null != cycleAt) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Get: cycle at {0} => not applicable", dateTime);
            }
            return this.CreateNotApplicable ();
          }
        }

        using (var transaction = session.BeginReadOnlyTransaction ("CycleIsProduction.NextProductionStart.NextCycle")) {

          var cycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .GetFirstBeginAfter (this.Machine, effectiveAfter.Subtract (TimeSpan.FromSeconds (1)));
          if ((null != cycle) && cycle.Begin.HasValue) {
            return this.CreateFinal (cycle.Begin.Value);
          }
          else { // No next cycle found
            Debug.Assert (cycleDetectionDateTime.HasValue);
            if (Bound.Compare<DateTime> (limit.Upper, cycleDetectionDateTime.Value) < 0) {
              // No cycle detection pending
              return this.CreateNoData ();
            }
            else {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: cycle detection date/time {0} after date/time {1} and no cycle at {1} => Hint", cycleDetectionDateTime, dateTime);
              }
              return this.CreateWithHint (new UtcDateTimeRange (cycleDetectionDateTime.Value));
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
