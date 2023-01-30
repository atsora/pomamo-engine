// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.Core.Cache;
using Lemoine.Extensions.Business;
using Lemoine.Collections;

namespace Lemoine.Plugin.NGoodCyclesIsProduction
{
  public class NextProductionStart
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    static readonly string CACHE_TIME_OUT_KEY = "NGoodCyclesIsProduction.NextProductionStart.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "NGoodCyclesIsProduction.NextProductionStart.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (3);

    static readonly string MAX_LIMIT_KEY =
      "NGoodCyclesIsProduction.NextProductionStart.MaxLimit";
    static readonly TimeSpan MAX_LIMIT_DEFAULT = TimeSpan.FromDays (3);

    static readonly string TIMEOUT_KEY = "NGoodCyclesIsProduction.NextProductionStart.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    readonly ILog log = LogManager.GetLogger (typeof (NextProductionStart).FullName);

    Configuration m_configuration;
    IMonitoredMachine m_monitoredMachine = null;

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
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

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      var maxLimit = Lemoine.Info.ConfigSet.LoadAndGet (MAX_LIMIT_KEY, MAX_LIMIT_DEFAULT);
      var maxLimitRange = new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime.Add (maxLimit));
      var correctedLimit = new UtcDateTimeRange (limit.Intersects (maxLimitRange));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("NGoodCyclesIsProduction.NextProductionStart")) {
          var range = new UtcDateTimeRange (new UtcDateTimeRange (dateTime)
            .Intersects (new UtcDateTimeRange (hint.Lower)));
          var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindOverlapsRangeAscending (this.Machine, range, m_configuration.NumberOfGoodCycles)
            .Where (c => IsAfter (c, dateTime))
            .LoadOperationSlots ();
          DateTime? afterResponse = null;
          DateTime? firstGoodCycleStart = null;
          DateTime? lastCycleEnd = null;
          int nbGoodCycles = 0;
          IOperationSlot operationSlot = null;
          foreach (var cycle in cycles) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"GetValid: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            var operationChangeDateTime = CheckOperationChange (cycle, ref operationSlot);
            if (operationChangeDateTime.HasValue) {
              var operationDetectionDateTime = GetOperationDetectionDateTime ();
              if (operationDetectionDateTime.HasValue
                && (operationChangeDateTime <= operationDetectionDateTime.Value)) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: operation change at {0} => reset everything", operationChangeDateTime);
                }
                nbGoodCycles = 0;
                afterResponse = cycle.DateTime;
                firstGoodCycleStart = null;
              }
              else {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: operation detection date/time {0} is before operation change {1}", operationDetectionDateTime, operationChangeDateTime);
                }
                var newHint = hint;
                if (afterResponse.HasValue) {
                  newHint = new UtcDateTimeRange (hint
                    .Intersects (new UtcDateTimeRange (afterResponse.Value)));
                }
                return this.CreateWithHint (newHint);
              }
            }

            // Check the potential final value is still in limit
            var cycleMinDateTime = GetMinDateTime (cycle);
            if ( (0 == nbGoodCycles) && (dateTime < cycleMinDateTime)) {
              if (!correctedLimit.ContainsElement (cycleMinDateTime)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: cycle min date/time {cycleMinDateTime} is out of limit {correctedLimit} => return NoData");
                }
                return this.CreateNoData ();
              }
            }

            // Check loading time
            if (lastCycleEnd.HasValue && cycle.Begin.HasValue) { // Check loading time
              var goodLoadingTime = IsGoodLoadingTime (cycle, lastCycleEnd.Value);
              switch (goodLoadingTime) {
              case GoodCycleExtensionResponse.OK:
                break;
              case GoodCycleExtensionResponse.KO:
                nbGoodCycles = 0;
                afterResponse = cycle.Begin.Value;
                firstGoodCycleStart = null;
                break;
              case GoodCycleExtensionResponse.POSTPONE:
                var newHint = hint;
                if (afterResponse.HasValue) {
                  newHint = new UtcDateTimeRange (hint
                    .Intersects (new UtcDateTimeRange (afterResponse.Value)));
                }
                return this.CreateWithHint (newHint);
              }
            }

            // Check once again the potential final value is still in limit
            // because the nbGoodCycles may be reset in the previous block of code
            if ((0 == nbGoodCycles) && (dateTime < cycleMinDateTime)) {
              if (!correctedLimit.ContainsElement (cycleMinDateTime)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: cycle min date/time {cycleMinDateTime} is out of limit {correctedLimit} => return NoData");
                }
                return this.CreateNoData ();
              }
            }

            if ((0 == nbGoodCycles) && cycle.Begin.HasValue) {
              afterResponse = cycle.Begin.Value;
            }

            // Check cycle
            var goodCycle = IsGoodCycle (cycle);
            switch (goodCycle) {
            case GoodCycleExtensionResponse.OK:
              ++nbGoodCycles;
              if (!firstGoodCycleStart.HasValue) {
                firstGoodCycleStart = cycleMinDateTime;
              }
              if (m_configuration.NumberOfGoodCycles <= nbGoodCycles) {
                Debug.Assert (firstGoodCycleStart.HasValue);
                return this.CreateFinal (firstGoodCycleStart.Value);
              }
              lastCycleEnd = cycle.End;
              break;
            case GoodCycleExtensionResponse.KO:
              nbGoodCycles = 0;
              if (cycle.Full) { // If partial, it may be ok in the future
                afterResponse = cycle.DateTime;
              }
              firstGoodCycleStart = null;
              break;
            case GoodCycleExtensionResponse.POSTPONE:
              var newHint = hint;
              if (afterResponse.HasValue) {
                newHint = new UtcDateTimeRange (hint
                  .Intersects (new UtcDateTimeRange (afterResponse.Value)));
              }
              return this.CreateWithHint (newHint);
            }
          } // foreach cycle
          if (afterResponse.HasValue) {
            if (!correctedLimit.ContainsElement (afterResponse.Value)) {
              return this.CreateNoData ();
            }
            else {
              var newHint = new UtcDateTimeRange (hint
                .Intersects (new UtcDateTimeRange (afterResponse.Value)));
              return this.CreateWithHint (newHint);
            }
          }
          else {
            var cycleDetectionDateTime = GetCycleDetectionDateTime ();
            if (cycleDetectionDateTime.HasValue) {
              if (Bound.Compare<DateTime> (correctedLimit.Upper, cycleDetectionDateTime.Value) < 0) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: no cycle in {0}-{1}", dateTime, correctedLimit.Upper);
                }
                return this.CreateNoData ();
              }
              else {
                var newHint = new UtcDateTimeRange (hint
                  .Intersects (new UtcDateTimeRange (cycleDetectionDateTime.Value)));
                return this.CreateWithHint (newHint);
              }
            }
            else {
              return this.CreateWithHint (hint);
            }
          }
        }
      }
    }

    DateTime? CheckOperationChange (IOperationCycle operationCycle, ref IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationCycle);

      if (null == operationCycle.OperationSlot) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("CheckOperationChange: operation cycle {0} is not associated to any operation slot");
        }
        return operationCycle.DateTime;
      }

      if (null == operationSlot) {
        operationSlot = operationCycle.OperationSlot;
      }
      var operation = operationSlot.Operation;
      if (null == operation) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("CheckOperationChange: operation slot is not associated to any operation");
        }
        return operationCycle.DateTime;
      }

      Debug.Assert (null != operationSlot);
      if ((null == operationCycle.OperationSlot)
        || (operationCycle.OperationSlot.Id != operationSlot.Id)) {
        if (!operationSlot.DateTimeRange.Upper.HasValue) {
          log.FatalFormat ("CheckOperationChange: operation cycle {0} is not associated to operation slot {1} although it should be", operationCycle.Id, operationSlot.Id);
          throw new Exception ("Invalid database data operation cycle VS operation slot");
        }
        var nextOperationSlotsRange =
          new UtcDateTimeRange (operationSlot.DateTimeRange.Upper.Value,
            operationCycle.DateTime, "[]");
        var nextOperationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRange (this.Machine, nextOperationSlotsRange);
        if (!nextOperationSlots.Any ()) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckOperationChange: no operation slots in range {0} => return false", nextOperationSlotsRange);
          }
          return operationSlot.DateTimeRange.Upper.Value;
        }
        foreach (var nextOperationSlot in nextOperationSlots) {
          if (!Bound.Equals<DateTime> (operationSlot.DateTimeRange.Upper,
            nextOperationSlot.DateTimeRange.Lower)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckOperationChange: gap {0}-{1} in operation slot",
                operationSlot.DateTimeRange.Upper, nextOperationSlot.DateTimeRange.Lower);
            }
            return operationSlot.DateTimeRange.Upper.Value;
          }
          if ((null == nextOperationSlot.Operation)
            || (((IDataWithId)nextOperationSlot.Operation).Id != ((IDataWithId)operationSlot.Operation).Id)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckOperationChange: operation change at {0}", operationSlot.DateTimeRange.Upper);
            }
            return operationSlot.DateTimeRange.Upper.Value;
          }
          operationSlot = nextOperationSlot;
          Debug.Assert (null != operationSlot.Operation);
          Debug.Assert (((IDataWithId)operationSlot.Operation).Id == ((IDataWithId)operation).Id);
        }
        Debug.Assert (operationSlot.Id == operationCycle.OperationSlot.Id);
      }
      return null;
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
      var operationDetectionDateTime = Business.ServiceProvider
        .Get<DateTime?> (operationDetectionStatusRequest);
      return operationDetectionDateTime;
    }

    DateTime GetMinDateTime (IOperationCycle cycle)
    {
      if (cycle.Begin.HasValue) {
        return cycle.Begin.Value;
      }
      else {
        Debug.Assert (cycle.End.HasValue);
        return cycle.End.Value;
      }
    }

    bool IsAfter (IOperationCycle cycle, DateTime dateTime)
    {
      return dateTime <= GetMinDateTime (cycle);
    }

    GoodCycleExtensionResponse IsGoodCycle (IOperationCycle cycle)
    {
      return cycle.IsGood (m_configuration.MaxMachiningDurationMultiplicator);
    }

    GoodCycleExtensionResponse IsGoodLoadingTime (IOperationCycle cycle, DateTime start)
    {
      return cycle.IsGoodLoadingTime (start, m_configuration.MaxLoadingDurationMultiplicator);
    }

    IMonitoredMachine GetMonitoredMachine ()
    {
      if (null == m_monitoredMachine) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (this.Machine.Id);
        }
      }
      return m_monitoredMachine;
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
