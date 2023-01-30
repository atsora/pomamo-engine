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
using Lemoine.Extensions.Database;
using Lemoine.Extensions.Business;
using Lemoine.Collections;

namespace Lemoine.Plugin.NGoodCyclesIsProduction
{
  /// <summary>
  /// Last production end (consider here only the full cycles)
  /// </summary>
  public class LastProductionEnd
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (LastProductionEnd).FullName);

    static readonly string CACHE_TIME_OUT_KEY = "NGoodCyclesIsProduction.LastProductionEnd.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "NGoodCyclesIsProduction.LastProductionEnd.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (3);

    static readonly string TIMEOUT_KEY = "NGoodCyclesIsProduction.LastProductionEnd.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    Configuration m_configuration;
    IMonitoredMachine m_monitoredMachine = null;

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
      DateTime? detectionDateTime = GetDetectionDateTime ();
      if (!detectionDateTime.HasValue) {
        log.Debug ("Get: no detection date/time");
        return DynamicTimeApplicableStatus.Pending;
      }
      else if (detectionDateTime.Value < dateTime) { // cycleDetectionDateTime.HasValue
        return DynamicTimeApplicableStatus.Pending;
      }

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
      Debug.Assert (limit.ContainsElement (dateTime));

      DateTime? detectionDateTime = GetDetectionDateTime ();
      if (!detectionDateTime.HasValue) {
        log.Debug ("Get: no detection date/time");
        return this.CreatePending ();
      }
      else { // cycleDetectionDateTime.HasValue
        if (dateTime <= detectionDateTime.Value) { // Ok
          return GetValid (dateTime, hint);
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

    DateTime? GetDetectionDateTime ()
    {
      var cycleDetectionDateTime = GetCycleDetectionDateTime ();
      if (!cycleDetectionDateTime.HasValue) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetDetectionDateTime: cycle detection date/time is null");
        }
        return null;
      }
      var operationDetectionDateTime = GetOperationDetectionDateTime ();
      if (!operationDetectionDateTime.HasValue) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetDetectionDateTime: operation detection date/time is null");
        }
        return null;
      }
      return cycleDetectionDateTime.Value < operationDetectionDateTime.Value
        ? cycleDetectionDateTime.Value
        : operationDetectionDateTime.Value;
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

    IDynamicTimeResponse GetValid (DateTime dateTime, UtcDateTimeRange hint)
    {
      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("NGoodCyclesIsProduction.LastProductionEnd")) {
          DateTime? end = null;
          IOperationCycle previousCycle = null;
          int nGoodCycles = 0;
          var range = new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime, "[]");
          var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindFullOverlapsRangeDescending (this.Machine, range, m_configuration.NumberOfGoodCycles);
          IOperationSlot operationSlot = null;
          foreach (var cycle in cycles) {
            Debug.Assert (cycle.End.HasValue);
            Debug.Assert (cycle.End.Value <= dateTime);

            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"GetValid: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if ((null != previousCycle)
                && cycle.End.HasValue
                && previousCycle.Begin.HasValue) {
              var goodLoadingTime = previousCycle.IsGoodLoadingTime (cycle.End.Value,
                m_configuration.MaxLoadingDurationMultiplicator);
              switch (goodLoadingTime) {
                case GoodCycleExtensionResponse.OK:
                  break;
                case GoodCycleExtensionResponse.KO:
                  nGoodCycles = 0;
                  end = null;
                  operationSlot = null;
                  previousCycle = cycle;
                  break;
                case GoodCycleExtensionResponse.POSTPONE:
                  if (end.HasValue) {
                    var newHint = new UtcDateTimeRange (hint
                      .Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), end.Value, "[]")));
                    return this.CreateWithHint (newHint);
                  }
                  else {
                    return this.CreateWithHint (hint);
                  }
              }
            }
            if (!CheckOperationChange (cycle, ref operationSlot)) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("GetValid: operation change at cycle {0}", cycle.Id);
              }
              nGoodCycles = 0;
              end = null;
            }
            var good = cycle.IsGood (m_configuration.MaxMachiningDurationMultiplicator);
            switch (good) {
              case GoodCycleExtensionResponse.OK:
                Debug.Assert (cycle.Begin.HasValue);
                if (0 == nGoodCycles) {
                  end = cycle.End.Value;
                }
                Debug.Assert (end.HasValue);
                if (!hint.ContainsElement (end.Value)) {
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("GetValid: hint {0} does not contain {1}, give up", hint, end);
                  }
                  return this.CreateNoData ();
                }
                ++nGoodCycles;
                if (m_configuration.NumberOfGoodCycles <= nGoodCycles) {
                  return this.CreateFinal (end.Value);
                }
                else { // Number of good cycles was not reached yet
                }
                break;
              case GoodCycleExtensionResponse.KO:
                nGoodCycles = 0;
                end = null;
                operationSlot = null;
                break;
              case GoodCycleExtensionResponse.POSTPONE:
                if (end.HasValue) {
                  var newHint = new UtcDateTimeRange (hint
                    .Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), end.Value, "[]")));
                  return this.CreateWithHint (newHint);
                }
                else if (cycle.End.HasValue) {
                  var newHint = new UtcDateTimeRange (hint
                    .Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), cycle.End.Value, "[]")));
                  return this.CreateWithHint (newHint);
                }
                else {
                  return this.CreateWithHint (hint);
                }
            }
            previousCycle = cycle;
          } // Loop on cycles

          log.DebugFormat ("GetValid: no enough good cycles before {0} => NoData", dateTime);
          return this.CreateNoData ();
        }
      }
    }

    bool CheckOperationChange (IOperationCycle operationCycle, ref IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationCycle);

      if (null == operationCycle.OperationSlot) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("CheckOperationChange: operation cycle {0} is not associated to any operation slot", operationCycle.Id);
        }
        return false;
      }

      if (null == operationSlot) {
        operationSlot = operationCycle.OperationSlot;
      }
      var operation = operationSlot.Operation;
      if (null == operation) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("CheckOperationChange: operation slot is not associated to any operation");
        }
        return false;
      }

      Debug.Assert (null != operationSlot);
      if ((null == operationCycle.OperationSlot) || (operationCycle.OperationSlot.Id != operationSlot.Id)) {
        if (!operationSlot.DateTimeRange.Lower.HasValue) {
          log.FatalFormat ("CheckOperationChange: operation cycle {0} is not associated to operation slot {1} although it should be", operationCycle.Id, operationSlot.Id);
          throw new Exception ("Invalid database data operation cycle VS operation slot");
        }
        UtcDateTimeRange previousOperationSlotsRange;
        if (Bound.Equals<DateTime> (operationCycle.DateTime, operationSlot.DateTimeRange.Lower.Value)) {
          log.WarnFormat ("CheckOperationChange: operation cycle ends at an end of an operation slot");
          previousOperationSlotsRange = new UtcDateTimeRange (operationCycle.DateTime.Subtract (TimeSpan.FromSeconds (1)),
            operationSlot.DateTimeRange.Lower.Value, "[)");
        }
        else {
          previousOperationSlotsRange =
            new UtcDateTimeRange (operationCycle.DateTime, operationSlot.DateTimeRange.Lower.Value, "[)");
        }
        Debug.Assert (!previousOperationSlotsRange.IsEmpty ());
        var previousOperationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRange (this.Machine, previousOperationSlotsRange);
        if (!previousOperationSlots.Any ()) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckOperationChangeBefore: no operation slots in range {0} => return false", previousOperationSlotsRange);
          }
          return false;
        }
        foreach (var previousOperationSlot in previousOperationSlots) {
          if (!Bound.Equals<DateTime> (operationSlot.DateTimeRange.Lower,
            previousOperationSlot.DateTimeRange.Upper)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckOperationChange: gap {0}-{1} in operation slot",
                previousOperationSlot.DateTimeRange.Upper, operationSlot.DateTimeRange.Lower);
            }
            return false;
          }
          if ((null == previousOperationSlot.Operation)
            || (((IDataWithId)previousOperationSlot.Operation).Id != ((IDataWithId)operation).Id)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckOperationChange: operation change at {0}", operationSlot.DateTimeRange.Lower);
            }
            return false;
          }
          operationSlot = previousOperationSlot;
        }
        Debug.Assert (operationSlot.Id == operationCycle.OperationSlot.Id);
      }
      return true;
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
      if (string.IsNullOrEmpty (this.Name)) { // The configuration is loaded in Name.get
        return false;
      }
      Debug.Assert (null != m_configuration);
      return m_configuration.CheckMachineFilter (machine);
    }
  }
}
