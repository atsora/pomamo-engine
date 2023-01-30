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
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Core.Cache;
using Lemoine.Extensions.Business;
using Lemoine.Collections;

namespace Lemoine.Plugin.NGoodCyclesIsProduction
{
  public class NextProductionEnd
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    static readonly string CACHE_TIME_OUT_KEY = "NGoodCyclesIsProduction.NextProductionEnd.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly string APPLICABLE_TIME_SPAN_KEY =
      "NGoodCyclesIsProduction.NextProductionEnd.ApplicableTimeSpan";
    static readonly TimeSpan APPLICABLE_TIME_SPAN_DEFAULT = TimeSpan.FromDays (3);

    static readonly string MAX_LIMIT_KEY =
      "NGoodCyclesIsProduction.NextProductionEnd.MaxLimit";
    static readonly TimeSpan MAX_LIMIT_DEFAULT = TimeSpan.FromDays (3);

    static readonly string TIMEOUT_KEY = "NGoodCyclesIsProduction.NextProductionEnd.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    readonly ILog log = LogManager.GetLogger (typeof (NextProductionEnd).FullName);

    Configuration m_configuration;
    IMonitoredMachine m_monitoredMachine;
    DateTime? m_operationDetectionDateTime;

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
      m_operationDetectionDateTime = null;

      Debug.Assert (limit.ContainsElement (dateTime));

      DateTime? detectionDateTime = GetDetectionDateTime ();
      if (!detectionDateTime.HasValue) {
        log.Debug ("Get: no cycle detection date/time");
        return this.CreatePending ();
      }
      else { // cycleDetectionDateTime.HasValue
        if (dateTime <= detectionDateTime.Value) { // Ok
          return GetValid (dateTime, detectionDateTime.Value, hint, limit);
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
      if (m_operationDetectionDateTime.HasValue) {
        return m_operationDetectionDateTime.Value;
      }
      var operationDetectionStatusRequest = new Lemoine.Business.Operation
        .OperationDetectionStatus (this.Machine);
      m_operationDetectionDateTime = Business.ServiceProvider
        .Get<DateTime?> (operationDetectionStatusRequest);
      return m_operationDetectionDateTime;
    }

    IDynamicTimeResponse GetValid (DateTime dateTime, DateTime detectionDateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      var maxLimit = Lemoine.Info.ConfigSet.LoadAndGet (MAX_LIMIT_KEY, MAX_LIMIT_DEFAULT);
      var maxLimitRange = new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime.Add (maxLimit));
      var correctedLimit = new UtcDateTimeRange (limit.Intersects (maxLimitRange));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("NGoodCyclesIsProduction.NextProductionEnd")) {
          var initialOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindAt (this.Machine, dateTime);
          var operationSlot = initialOperationSlot;
          if (null == operationSlot) {
            log.Error ($"GetValid: no operation slot at {dateTime} => not applicable");
            return this.CreateNotApplicable ();
          }
          var operation = operationSlot.Operation;
          if (null == operation) {
            log.Error ($"GetValid: no operation at {dateTime} => not applicable");
            return this.CreateNotApplicable ();
          }

          var range = new UtcDateTimeRange (dateTime, detectionDateTime, "[]");
          var afterCycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindOverlapsRangeAscending (this.Machine, range, m_configuration.NumberOfGoodCycles)
            .LoadOperationSlots ();
          DateTime lastDateTime = dateTime;
          int nbGoodCyclesAfter = 0;
          IOperationCycle firstCycle = null;
          foreach (var afterCycle in afterCycles) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"GetValid: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (null == firstCycle) {
              firstCycle = afterCycle;
            }
            var operationChangeDateTime = CheckOperationChangeAfter (afterCycle, operation, ref operationSlot);
            if (operationChangeDateTime.HasValue) {
              var operationDetectionDateTime = GetOperationDetectionDateTime ();
              if (operationDetectionDateTime.HasValue
                && (operationChangeDateTime <= operationDetectionDateTime.Value)) {
                return Stop (dateTime, operation, initialOperationSlot, nbGoodCyclesAfter, operationChangeDateTime.Value, firstCycle);
              }
              else {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetValid: operation detection date/time {operationDetectionDateTime} is before operation change {operationChangeDateTime}");
                }
                var newHint = new UtcDateTimeRange (hint
                  .Intersects (new UtcDateTimeRange (lastDateTime)));
                return this.CreateWithHint (newHint);
              }
            }

            // Check loading time
            if (afterCycle.Begin.HasValue) {
              var goodLoadingTime = IsGoodLoadingTime (afterCycle, lastDateTime);
              switch (goodLoadingTime) {
              case GoodCycleExtensionResponse.OK:
                break;
              case GoodCycleExtensionResponse.KO:
                return Stop (dateTime, operation, initialOperationSlot, nbGoodCyclesAfter, lastDateTime, firstCycle);
              case GoodCycleExtensionResponse.POSTPONE:
                var newHint = new UtcDateTimeRange (hint
                  .Intersects (new UtcDateTimeRange (lastDateTime)));
                return this.CreateWithHint (newHint);
              }
            }

            // Check cycle
            if (!afterCycle.End.HasValue) {
              return Stop (dateTime, operation, initialOperationSlot, nbGoodCyclesAfter, lastDateTime, firstCycle);
            }
            var goodCycle = IsGoodCycle (afterCycle);
            switch (goodCycle) {
            case GoodCycleExtensionResponse.OK:
              Debug.Assert (afterCycle.End.HasValue);
              ++nbGoodCyclesAfter;
              lastDateTime = afterCycle.End.Value;
              break;
            case GoodCycleExtensionResponse.KO:
              return Stop (dateTime, operation, initialOperationSlot, nbGoodCyclesAfter, lastDateTime, firstCycle);
            case GoodCycleExtensionResponse.POSTPONE:
              // TODO:
              break;
            }

            // Check the potential final value is still in limit
            if (!correctedLimit.ContainsElement (lastDateTime)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: new date/time {lastDateTime} is out of limit {correctedLimit} => return NoData");
              }
              return this.CreateNoData ();
            }
          }
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetValid: {0} good cycles detected until {1} in range {2}", nbGoodCyclesAfter, lastDateTime, range);
          }

          if (Bound.Compare<DateTime> (correctedLimit.Upper, detectionDateTime) < 0) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetValid: there won't be any cycle before limit {0}", correctedLimit.Upper);
            }
            return this.CreateNoData ();
          }
          else {
            return this.CreateWithHint (new UtcDateTimeRange (detectionDateTime));
          }
        }
      }
    }

    DateTime? CheckOperationChangeAfter (IOperationCycle afterCycle, IOperation operation, ref IOperationSlot operationSlot)
    {
      Debug.Assert (null != afterCycle);
      Debug.Assert (null != operation);
      Debug.Assert (null != operationSlot);

      if ((null == afterCycle.OperationSlot) || (afterCycle.OperationSlot.Id != operationSlot.Id)) {
        if (!operationSlot.DateTimeRange.Upper.HasValue) {
          if ((null == operationSlot.Operation)
            && (null != afterCycle.OperationSlot)
            && afterCycle.Begin.HasValue
            && (Bound.Compare<DateTime> (afterCycle.Begin.Value, afterCycle.OperationSlot.DateTimeRange.Upper) < 0)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CheckOperationChangeAfter: specific case when an operation cycle is associated to the operation slot before since the current operation slot has no operation that is associated to it");
            }
            return null;
          }
          else if ((null != afterCycle.OperationSlot) && (null != operationSlot.Operation)
            && afterCycle.Begin.HasValue
            && (Bound.Compare<DateTime> (afterCycle.Begin.Value, afterCycle.OperationSlot.DateTimeRange.Upper) < 0)) {
            if ((((IDataWithId)operationSlot.Operation).Id == afterCycle.OperationSlot.Id)
              && Bound.Equals<DateTime> (afterCycle.OperationSlot.DateTimeRange.Upper, operationSlot.DateTimeRange.Lower)) { // Same operation all the time
              if (log.IsWarnEnabled) {
                log.Warn ($"CheckOperationChangeAfter: same operation but cycle was associated to previous operation slot => continue though");
              }
            }
            else { // new operation
              if (log.IsInfoEnabled) {
                log.Info ($"CheckOperationChangeAfter: operation cyle {afterCycle.Id} Full={afterCycle.Full} is on two operation slots with different operations and is associated to the previous one => return its cycle end");
              }
              if (afterCycle.End.HasValue) {
                return afterCycle.End;
              }
              else {
                log.Fatal ($"CheckOperationChangeAfter: operation cyle {afterCycle.Id} Full={afterCycle.Full} with no end is on two operation slots with different operations and is associated to the previous one => unexpected");
                throw new Exception ("Invalid database data operation cycle VS operation slot");
              }
            }
          }
          else { // Potentially not a normal case
            log.Fatal ($"CheckOperationChangeAfter: operation cycle id={afterCycle.Id} at {afterCycle.DateTime}, {afterCycle.Begin}-{afterCycle.End} full={afterCycle.Full} on operation slot {afterCycle.OperationSlot?.Id} {afterCycle.OperationSlot?.DateTimeRange} is not associated to operation slot {operationSlot.Id} range={operationSlot.DateTimeRange} although it should be");
            if (null == afterCycle.OperationSlot) {
              log.Fatal ($"CheckOperationChangeAfter: operation cycle id={afterCycle.Id} has no associated operation slot => return null");
              return null;
            }
            else if (Bound.Compare<DateTime> (afterCycle.OperationSlot.DateTimeRange.Upper, operationSlot.DateTimeRange.Lower) < 0) {
              log.Fatal ($"CheckOperationChangeAfter: operation cycle id={afterCycle.Id} belongs to an operation slot {afterCycle.OperationSlot.Id} that is before operation slot {operationSlot.Id} => return null");
              return null;
            }
            else {
              log.Fatal ($"CheckOperationChangeAfter: operation cycle id={afterCycle.Id} belongs to an operation slot {afterCycle.OperationSlot.Id} on range {afterCycle.OperationSlot.DateTimeRange} that is after operation slot {operationSlot.Id} with no upper value => throw an exception");
              throw new Exception ("Invalid database data operation cycle VS operation slot");
            }
          }
        }
        var nextOperationSlotsRange =
          new UtcDateTimeRange (operationSlot.DateTimeRange.Upper.Value,
            afterCycle.DateTime, "[]");
        var nextOperationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRange (this.Machine, nextOperationSlotsRange);
        foreach (var nextOperationSlot in nextOperationSlots) {
          if (!Bound.Equals<DateTime> (operationSlot.DateTimeRange.Upper,
            nextOperationSlot.DateTimeRange.Lower)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckOperationChangeAfter: gap {0}-{1} in operation slot",
                operationSlot.DateTimeRange.Upper, nextOperationSlot.DateTimeRange.Lower);
            }
            return operationSlot.DateTimeRange.Upper.Value;
          }
          if ((null == nextOperationSlot.Operation)
            || (((IDataWithId)nextOperationSlot.Operation).Id != ((IDataWithId)operation).Id)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CheckOperationChangeAfter: operation change at {operationSlot.DateTimeRange.Upper}");
            }
            return operationSlot.DateTimeRange.Upper.Value;
          }
          operationSlot = nextOperationSlot;
        }
        Debug.Assert (operationSlot.Id == afterCycle.OperationSlot.Id);
      }
      return null;
    }

    bool CheckOperationChangeBefore (IOperationCycle beforeCycle, IOperation operation, ref IOperationSlot operationSlot)
    {
      Debug.Assert (null != beforeCycle);
      Debug.Assert (null != operation);
      Debug.Assert (null != operationSlot);

      if ((null == beforeCycle.OperationSlot) || (beforeCycle.OperationSlot.Id != operationSlot.Id)) {
        if (!operationSlot.DateTimeRange.Lower.HasValue) {
          log.FatalFormat ("CheckOperationChangeBefore: operation cycle {0} is not associated to operation slot {1} although it should be", beforeCycle.Id, operationSlot.Id);
          throw new Exception ("Invalid database data operation cycle VS operation slot");
        }
        var previousOperationSlotsRange =
          new UtcDateTimeRange (beforeCycle.DateTime, operationSlot.DateTimeRange.Lower.Value, "[)");
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
              log.DebugFormat ("CheckOperationChangeBefore: gap {0}-{1} in operation slot",
                previousOperationSlot.DateTimeRange.Upper, operationSlot.DateTimeRange.Lower);
            }
            return false;
          }
          if ((null == previousOperationSlot.Operation)
            || (((IDataWithId)previousOperationSlot.Operation).Id != ((IDataWithId)operation).Id)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckOperationChangeBefore: operation change at {0}", operationSlot.DateTimeRange.Lower);
            }
            return false;
          }
          operationSlot = previousOperationSlot;
        }
        Debug.Assert (operationSlot.Id == beforeCycle.OperationSlot.Id);
      }
      return true;
    }

    IDynamicTimeResponse Stop (DateTime dateTime, IOperation operation, IOperationSlot initialOperationSlot, int nbGoodCyclesAfter, DateTime stopDateTime, IOperationCycle firstCycleAfter)
    {
      if (m_configuration.NumberOfGoodCycles <= nbGoodCyclesAfter) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("Stop: {0} good cycles found, stop at {1}", nbGoodCyclesAfter, stopDateTime);
        }
        return this.CreateFinal (stopDateTime);
      }

      // Check the cycles before to check if there are enough cycles
      var operationSlot = initialOperationSlot;
      int nbGoodCycles = nbGoodCyclesAfter;
      int missingCycles = m_configuration.NumberOfGoodCycles - nbGoodCyclesAfter;
      Debug.Assert (0 < missingCycles);
      var range = new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime, "[]");
      var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
        .FindFullOverlapsRangeDescending (this.Machine, range, missingCycles);
      var previousCycle = firstCycleAfter;
      foreach (var cycle in cycles) {
        if (false == CheckOperationChangeBefore (cycle, operation, ref operationSlot)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Stop: operation change at cycle {0}", previousCycle.Id);
          }
          return NotEnoughGoodCycles (nbGoodCycles);
        }
        if (cycle.End.HasValue && previousCycle.Begin.HasValue) { // First
          var goodLoadingTime = IsGoodLoadingTime (previousCycle, cycle.End.Value);
          switch (goodLoadingTime) {
          case GoodCycleExtensionResponse.OK:
            break;
          case GoodCycleExtensionResponse.KO:
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Stop: bad loading time for cycle {0}", previousCycle.Id);
            }
            return NotEnoughGoodCycles (nbGoodCycles);
          case GoodCycleExtensionResponse.POSTPONE:
            var newHint = new UtcDateTimeRange (stopDateTime, stopDateTime, "[]");
            return this.CreateWithHint (newHint);
          }
        }

        var goodCycle = IsGoodCycle (cycle);
        switch (goodCycle) {
        case GoodCycleExtensionResponse.OK:
          ++nbGoodCycles;
          if (m_configuration.NumberOfGoodCycles <= nbGoodCycles) {
            Debug.Assert (m_configuration.NumberOfGoodCycles == nbGoodCycles);
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Stop: enough good cycles detected");
            }
            return this.CreateFinal (stopDateTime);
          }
          previousCycle = cycle;
          break;
        case GoodCycleExtensionResponse.KO:
          return NotEnoughGoodCycles (nbGoodCycles);
        case GoodCycleExtensionResponse.POSTPONE:
          var newHint = new UtcDateTimeRange (stopDateTime, stopDateTime, "[]");
          return this.CreateWithHint (newHint);
        }
      }
      Debug.Assert (nbGoodCycles < m_configuration.NumberOfGoodCycles);
      return NotEnoughGoodCycles (nbGoodCycles);
    }

    IDynamicTimeResponse NotEnoughGoodCycles (int nbGoodCycles)
    {
      if (log.IsWarnEnabled) {
        log.WarnFormat ("NotEnoughGoodCycles: only {0} good cycles detected => Not Applicable", nbGoodCycles);
      }
      return this.CreateNotApplicable ();
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
