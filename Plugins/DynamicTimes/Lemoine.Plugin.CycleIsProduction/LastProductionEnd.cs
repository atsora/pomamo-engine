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
using Lemoine.Extensions.Database;

namespace Lemoine.Plugin.CycleIsProduction
{
  /// <summary>
  /// Last production end (consider here only the full cycles)
  /// </summary>
  public class LastProductionEnd
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (LastProductionEnd).FullName);

    static readonly string CACHE_TIME_OUT_KEY = "CycleIsProduction.LastProductionEnd.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "CycleIsProduction.LastProductionEnd.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (3);

    Configuration m_configuration;

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        var suffix = "LastProductionEnd";
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
      Debug.Assert (limit.ContainsElement (dateTime));

      DateTime? cycleDetectionDateTime = GetCycleDetectionDateTime ();
      if (!cycleDetectionDateTime.HasValue) {
        log.Debug ("Get: no cycle detection date/time");
        return this.CreatePending ();
      }
      else { // cycleDetectionDateTime.HasValue
        if (dateTime <= cycleDetectionDateTime.Value) { // Ok
          return GetValid (dateTime);
        }
        else {
          log.Debug ("Get: cycle detection date/time in the past => pending");
          // Do not try to return an 'After' date/time
          // because it does not look necessary for the moment
          // It may change in the future
          return this.CreatePending ();
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

    IDynamicTimeResponse GetValid (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("CycleIsProduction.LastProductionEnd.CycleAt")) {
          var cycleAt = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAt (this.Machine, dateTime);
          if (null != cycleAt) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Get: cycle at {0} => not applicable", dateTime);
            }
            return this.CreateNotApplicable ();
          }
        }


        using (var transaction = session.BeginReadOnlyTransaction ("CycleIsProduction.LastProductionEnd.PreviousCycle")) {
          var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .GetLastFullCyclesBefore (this.Machine, dateTime, 1);
          if (cycles.Any ()) {
            var cycle = cycles.First ();
            Debug.Assert (cycle.End.HasValue);
            Debug.Assert (cycle.End.Value <= dateTime);
            return this.CreateFinal (cycle.End.Value);
          }
          else {
            log.DebugFormat ("GetValid: no full cycle before {0}", dateTime);
            return this.CreateNoData ();
          }
        }
      }
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
      this.Machine = machine;
      log = LogManager.GetLogger ($"{typeof (LastProductionEnd).FullName}.{this.Machine?.Id}");
      if (string.IsNullOrEmpty (this.Name)) { // The configuration is loaded in Name.get
        return false;
      }
      Debug.Assert (null != m_configuration);
      return m_configuration.CheckMachineFilter (machine);
    }
  }
}
