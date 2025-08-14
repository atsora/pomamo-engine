// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.Operation;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Response of a PartProductionRange request
  /// </summary>
  public struct PartProductionRangeResponse
    : Lemoine.Extensions.Business.Operation.IPartProductionDataRange
  {
    /// <summary>
    /// Range
    /// </summary>
    public UtcDateTimeRange Range { get; private set; }

    /// <summary>
    /// Number of completed pieces
    /// </summary>
    public double NbPieces { get; set; }

    /// <summary>
    /// Target
    /// </summary>
    public double? Goal { get; set; }

    /// <summary>
    /// In progress ?
    /// </summary>
    public bool InProgress { get; set; }

    /// <summary>
    /// Operations
    /// </summary>
    public ICollection<IOperation> Operations { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="range"></param>
    /// <param name="nbPieces"></param>
    /// <param name="goal"></param>
    /// <param name="inProgress"></param>
    /// <param name="operations"></param>
    public PartProductionRangeResponse (UtcDateTimeRange range, double nbPieces, double? goal, bool inProgress, ICollection<IOperation> operations)
    {
      this.Range = range;
      this.NbPieces = nbPieces;
      this.Goal = goal;
      this.InProgress = inProgress;
      this.Operations = operations;
    }
  }

  /// <summary>
  /// Request class to get both a number of made parts and a target/goal for a specific range
  /// </summary>
  public sealed class PartProductionRange
    : IRequest<PartProductionRangeResponse>
  {
    static readonly string IN_PROGRESS_CACHE_TIME_OUT_KEY = "Business.Operation.PartProductionRange.InProgressCacheTimeOut";
    static readonly TimeSpan IN_PROGRESS_CACHE_TIME_OUT_DEFAULT = TimeSpan.FromMinutes (5);

    readonly IMachine m_machine;
    IMonitoredMachine m_monitoredMachine;
    readonly UtcDateTimeRange m_range;
    readonly UtcDateTimeRange m_preLoadRange;
    readonly Func<IEnumerable<IObservationStateSlot>> m_observationStateSlotsPreLoader;
    DateTime? m_operationDetectionStatusCache = null;

    readonly ILog log = LogManager.GetLogger (typeof (PartProductionRange).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null, lower must be defined</param>
    /// <param name="preLoadRange">nullable</param>
    /// <param name="observationStateSlotsPreLoader">nullable</param>
    public PartProductionRange (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null, Func<IEnumerable<IObservationStateSlot>> observationStateSlotsPreLoader = null)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != range);
      Debug.Assert (range.IsEmpty () || range.Lower.HasValue);

      if (!range.IsEmpty () && !range.Lower.HasValue) {
        log.Error ($"PartProductionRange: range {range} must have a finite lower bound");
        throw new ArgumentException ("Range must have a finite lower bound", "range");
      }
      if (machine is null) {
        log.Error ($"PartProductionRange: machine is null");
        throw new ArgumentNullException ("machine");
      }

      m_machine = machine;
      m_range = range;
      m_preLoadRange = preLoadRange;
      m_observationStateSlotsPreLoader = observationStateSlotsPreLoader;

      log = LogManager.GetLogger ($"{typeof (PartProductionRange).FullName}.{m_machine.Id}");
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public PartProductionRangeResponse Get ()
    {
      try {
        m_operationDetectionStatusCache = null;

        if (log.IsDebugEnabled) {
          log.Debug ($"Get: range={m_range}");
        }

        var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
        m_monitoredMachine = ServiceProvider.Get (monitoredMachineBusinessRequest);
        if (null == m_monitoredMachine) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: machine with id {m_machine.Id} is not monitored");
          }
          return new PartProductionRangeResponse (m_range, 0, null, false, new List<IOperation> { });
        }

        if (m_range.IsEmpty ()) {
          log.Warn ($"Get: range is empty");
          return new PartProductionRangeResponse (m_range, 0, null, false, new List<IOperation> { });
        }

        var productionPeriodsRequest = new Lemoine.Business.MachineObservationState.ProductionPeriods (m_machine, m_range, m_preLoadRange, m_observationStateSlotsPreLoader);
        var productionPeriods = Lemoine.Business.ServiceProvider
          .Get (productionPeriodsRequest);

        var cycleCounterRequest = new CycleCounter (m_monitoredMachine, m_range, m_preLoadRange);
        var cycleCounter = ServiceProvider
          .Get (cycleCounterRequest);
        if (!cycleCounter.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: no operation was identified in range {m_range}");
          }
          return new PartProductionRangeResponse (m_range, 0, null, !IsStaticFromRange (), new List<IOperation> { });
        }

        var operations = cycleCounter
          .Select (x => x.Operation)
          .Where (x => !(x is null))
          .Distinct (new EqualityComparerDataWithId<IOperation> ())
          .ToList ();

        if (productionPeriods.All (x => x.Item2.HasValue && x.Item2.Value)) {
          // All periods is production
          var nbPieces = cycleCounter
            .Sum (x => GetNbPiecesFromCycles (x));
          var targets = cycleCounter
            .Where (x => !(x.Operation is null))
            .Select (x => GetTarget (x.Operation, x.Duration))
            .Where (x => x.HasValue);
          var target = targets.Any ()
            ? targets.Sum (x => x.Value)
            : (double?)null;
          var inProgress = GetInProgress (cycleCounter);
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: all observation slots are production, range={m_range} pieces={nbPieces} target={target}");
          }
          return new PartProductionRangeResponse (m_range, nbPieces, target, inProgress, operations);
        }

        if (productionPeriods.All (x => !x.Item2.HasValue || !x.Item2.Value)) {
          // None period is production
          var nbPieces = cycleCounter
            .Sum (x => GetNbPiecesFromCycles (x));
          var target = 0;
          var inProgress = GetInProgress (cycleCounter);
          return new PartProductionRangeResponse (m_range, nbPieces, target, inProgress, operations);
        }

        if (1 == cycleCounter.Count ()
          && m_range.Duration.HasValue) {
          var uniqueCycleCounterValue = cycleCounter.First ();
          if (uniqueCycleCounterValue.Duration.Equals (m_range.Duration)) {
            // Unique operation during the whole period
            return GetUniqueOperation (uniqueCycleCounterValue, productionPeriods);
          }
          var operationDetectionStatus = GetOperationDetectionStatus ();
          if (operationDetectionStatus.HasValue) {
            var rangeLimitToOperationDetection = new UtcDateTimeRange (m_range.Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (), operationDetectionStatus.Value)));
            if (rangeLimitToOperationDetection.Duration.HasValue
              && (rangeLimitToOperationDetection.Duration.Value <= uniqueCycleCounterValue.Duration)) {
              return GetUniqueOperation (uniqueCycleCounterValue, productionPeriods);
            }
          }
        }

        // Else multiple work pieces and/or periods
        var partProductionRangeResponse = new PartProductionRangeResponse (m_range, 0, null, false, operations);
        if (Bound.Compare<DateTime> (DateTime.UtcNow, m_range.Upper) < 0) {  // Present or future
          partProductionRangeResponse.InProgress = true;
        }
        var cycleCounterTotalDuration = TimeSpan.FromTicks (0);
        foreach (var productionPeriod in productionPeriods) {
          var intersection = new UtcDateTimeRange (productionPeriod.Item1
            .Intersects (m_range));
          cycleCounterRequest = new CycleCounter (m_monitoredMachine, intersection, m_preLoadRange);
          cycleCounter = ServiceProvider
            .Get (cycleCounterRequest);
          if (cycleCounter.Any ()) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: {cycleCounter.Count ()} for range {intersection}");
            }
            cycleCounterTotalDuration = cycleCounterTotalDuration
              .Add (TimeSpan.FromSeconds (cycleCounter.Sum (x => x.Duration.TotalSeconds)));
            partProductionRangeResponse.NbPieces += cycleCounter
              .Sum (x => GetNbPiecesFromCycles (x));
            if (productionPeriod.Item2.HasValue && productionPeriod.Item2.Value) {
              var targets = cycleCounter
                .Select (x => x.Operation)
                .Where (x => !(x is null))
                .Distinct ()
                .Select (operation => GetTarget (operation, LimitRangeToNow (intersection).Duration.Value))
                .Where (x => x.HasValue);
              if (targets.Any ()) {
                Debug.Assert (targets.All (x => x.HasValue));
                var target = targets
                  .Sum (x => x.Value);
                if (partProductionRangeResponse.Goal.HasValue) {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Get: for goal, add {target} to {partProductionRangeResponse.Goal.Value} for range {intersection}");
                  }
                  partProductionRangeResponse.Goal = partProductionRangeResponse.Goal.Value + target;
                }
                else {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Get: for goal, first target {target} for range {intersection}");
                  }
                  partProductionRangeResponse.Goal = target;
                }
              }
            }
            else { // Not a production
              if (!partProductionRangeResponse.Goal.HasValue) {
                if (operations.Any (x => IsStandardCycleDurationDefined (x))) {
                  partProductionRangeResponse.Goal = 0.0;
                }
              }
            } // End of Production test
            partProductionRangeResponse.InProgress |= cycleCounter.Any (x => x.InProgress);
          }
        }
        if (!partProductionRangeResponse.InProgress) {
          if (!m_range.Duration.HasValue || !m_range.Duration.Value.Equals (cycleCounterTotalDuration)) {
            partProductionRangeResponse.InProgress = !IsStaticFromRange ();
          }
        }
        return partProductionRangeResponse;
      }
      catch (Exception ex) {
        log.Error ("Get: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<PartProductionRangeResponse> GetAsync ()
    {
      try {
        m_operationDetectionStatusCache = null;

        if (log.IsDebugEnabled) {
          log.Debug ($"GetAsync: range={m_range}");
        }

        var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
        m_monitoredMachine = await ServiceProvider.GetAsync (monitoredMachineBusinessRequest);
        if (null == m_monitoredMachine) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: machine with id {m_machine.Id} is not monitored");
          }
          return new PartProductionRangeResponse (m_range, 0, null, false, new List<IOperation> { });
        }

        if (m_range.IsEmpty ()) {
          log.Warn ($"GetAsync: range is empty");
          return new PartProductionRangeResponse (m_range, 0, null, false, new List<IOperation> { });
        }

        var productionPeriodsRequest = new Lemoine.Business.MachineObservationState.ProductionPeriods (m_machine, m_range, m_preLoadRange, m_observationStateSlotsPreLoader);
        var productionPeriods = await Lemoine.Business.ServiceProvider
          .GetAsync (productionPeriodsRequest);

        var cycleCounterRequest = new CycleCounter (m_monitoredMachine, m_range, m_preLoadRange);
        var cycleCounter = await ServiceProvider
          .GetAsync (cycleCounterRequest);
        if (!cycleCounter.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: no operation was identified in range {m_range}");
          }
          return new PartProductionRangeResponse (m_range, 0, null, !IsStaticFromRange (), new List<IOperation> { });
        }

        var operations = cycleCounter
          .Select (x => x.Operation)
          .Where (x => !(x is null))
          .Distinct (new EqualityComparerDataWithId<IOperation> ())
          .ToList ();

        if (productionPeriods.All (x => x.Item2.HasValue && x.Item2.Value)) {
          // All periods is production
          var nbPieces = cycleCounter
            .Sum (x => GetNbPiecesFromCycles (x));
          var targets = cycleCounter
            .Where (x => !(x.Operation is null))
            .Select (x => GetTarget (x.Operation, x.Duration))
            .Where (x => x.HasValue);
          var target = targets.Any ()
            ? targets.Sum (x => x.Value)
            : (double?)null;
          var inProgress = GetInProgress (cycleCounter);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: all observation slots are production, range={m_range} pieces={nbPieces} target={target}");
          }
          return new PartProductionRangeResponse (m_range, nbPieces, target, inProgress, operations);
        }

        if (productionPeriods.All (x => !x.Item2.HasValue || !x.Item2.Value)) {
          // None period is production
          var nbPieces = cycleCounter
            .Sum (x => GetNbPiecesFromCycles (x));
          var target = 0;
          var inProgress = GetInProgress (cycleCounter);
          return new PartProductionRangeResponse (m_range, nbPieces, target, inProgress, operations);
        }

        if (1 == cycleCounter.Count ()
          && m_range.Duration.HasValue) {
          var uniqueCycleCounterValue = cycleCounter.First ();
          if (uniqueCycleCounterValue.Duration.Equals (m_range.Duration)) {
            // Unique operation during the whole period
            return GetUniqueOperation (uniqueCycleCounterValue, productionPeriods);
          }
          var operationDetectionStatus = GetOperationDetectionStatus ();
          if (operationDetectionStatus.HasValue) {
            var rangeLimitToOperationDetection = new UtcDateTimeRange (m_range.Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (), operationDetectionStatus.Value)));
            if (rangeLimitToOperationDetection.Duration.HasValue
              && (rangeLimitToOperationDetection.Duration.Value <= uniqueCycleCounterValue.Duration)) {
              return GetUniqueOperation (uniqueCycleCounterValue, productionPeriods);
            }
          }
        }

        // Else multiple work pieces and/or periods
        var partProductionRangeResponse = new PartProductionRangeResponse (m_range, 0, null, false, operations);
        if (Bound.Compare<DateTime> (DateTime.UtcNow, m_range.Upper) < 0) {  // Present or future
          partProductionRangeResponse.InProgress = true;
        }
        var cycleCounterTotalDuration = TimeSpan.FromTicks (0);
        foreach (var productionPeriod in productionPeriods) {
          var intersection = new UtcDateTimeRange (productionPeriod.Item1
            .Intersects (m_range));
          cycleCounterRequest = new CycleCounter (m_monitoredMachine, intersection, m_preLoadRange);
          cycleCounter = await ServiceProvider
            .GetAsync (cycleCounterRequest);
          if (cycleCounter.Any ()) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetAsync: {cycleCounter.Count ()} for range {intersection}");
            }
            cycleCounterTotalDuration = cycleCounterTotalDuration
              .Add (TimeSpan.FromSeconds (cycleCounter.Sum (x => x.Duration.TotalSeconds)));
            partProductionRangeResponse.NbPieces += cycleCounter
              .Sum (x => GetNbPiecesFromCycles (x));
            if (productionPeriod.Item2.HasValue && productionPeriod.Item2.Value) {
              var targets = cycleCounter
                .Select (x => x.Operation)
                .Where (x => !(x is null))
                .Distinct ()
                .Select (operation => GetTarget (operation, LimitRangeToNow (intersection).Duration.Value))
                .Where (x => x.HasValue);
              if (targets.Any ()) {
                Debug.Assert (targets.All (x => x.HasValue));
                var target = targets
                  .Sum (x => x.Value);
                if (partProductionRangeResponse.Goal.HasValue) {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"GetAsync: for goal, add {target} to {partProductionRangeResponse.Goal.Value} for range {intersection}");
                  }
                  partProductionRangeResponse.Goal = partProductionRangeResponse.Goal.Value + target;
                }
                else {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"GetAsync: for goal, first target {target} for range {intersection}");
                  }
                  partProductionRangeResponse.Goal = target;
                }
              }
            }
            else { // Not a production
              if (!partProductionRangeResponse.Goal.HasValue) {
                if (operations.Any (x => IsStandardCycleDurationDefined (x))) {
                  partProductionRangeResponse.Goal = 0.0;
                }
              }
            } // End of Production test
            partProductionRangeResponse.InProgress |= cycleCounter.Any (x => x.InProgress);
          }
        }
        if (!partProductionRangeResponse.InProgress) {
          if (!m_range.Duration.HasValue || !m_range.Duration.Value.Equals (cycleCounterTotalDuration)) {
            partProductionRangeResponse.InProgress = !IsStaticFromRange ();
          }
        }
        return partProductionRangeResponse;
      }
      catch (Exception ex) {
        log.Error ("GetAsync: exception", ex);
        throw;
      }
    }

    PartProductionRangeResponse GetUniqueOperation (CycleCounterValue uniqueCycleCounterValue, IEnumerable<(UtcDateTimeRange, bool?)> productionPeriods)
    {
      var nbPieces = GetNbPiecesFromCycles (uniqueCycleCounterValue);
      var productionDurationSeconds = productionPeriods
        .Where (s => s.Item2.HasValue && s.Item2.Value)
        .Sum (s => LimitRangeToNow (new UtcDateTimeRange (s.Item1.Intersects (m_range))).Duration.Value.TotalSeconds);
      var productionDuration = TimeSpan.FromSeconds (productionDurationSeconds);
      var target = uniqueCycleCounterValue.Operation is null
        ? null
        : GetTarget (uniqueCycleCounterValue.Operation, productionDuration);
      var inProgress = uniqueCycleCounterValue.InProgress;
      if (log.IsDebugEnabled) {
        log.Debug ($"GetUniqueOperation: range={m_range} pieces={nbPieces} target={target}");
      }
      var operations = uniqueCycleCounterValue.Operation is null
        ? new List<IOperation> ()
        : new List<IOperation> { uniqueCycleCounterValue.Operation };
      return new PartProductionRangeResponse (m_range, nbPieces, target, inProgress, operations);
    }


    bool GetInProgress (IEnumerable<CycleCounterValue> cycleCounterValues)
    {
      // Present or future => in progress
      var utcNow = DateTime.UtcNow;
      if (Bound.Compare<DateTime> (utcNow, m_range.Upper) < 0) {  // Present or future
        return true;
      }

      if (cycleCounterValues.Any (x => x.InProgress)) {
        return true;
      }

      var totalDuration = TimeSpan.FromSeconds (cycleCounterValues
        .Sum (x => x.Duration.TotalSeconds));
      if (m_range.Duration.HasValue && m_range.Duration.Value.Equals (totalDuration)) {
        return false;
      }

      return !IsStaticFromRange ();
    }

    UtcDateTimeRange LimitRangeToNow (UtcDateTimeRange range)
    {
      var now = DateTime.UtcNow;
      if (Bound.Compare<DateTime> (range.Upper, now) <= 0) {
        return range;
      }
      else {
        return new UtcDateTimeRange (range.Lower, now, range.LowerInclusive, false);
      }
    }

    bool IsStandardCycleDurationDefined (IOperation operation)
    {
      Debug.Assert (null != m_monitoredMachine);

      var standardCycleDurationRequest = new StandardCycleDuration (m_monitoredMachine, operation);
      var standardCycleDuration = Lemoine.Business.ServiceProvider
        .Get (standardCycleDurationRequest);
      return standardCycleDuration.HasValue;
    }

    bool IsStaticFromRange ()
    {
      if (m_range.IsEmpty ()) {
        return true;
      }

      var cycleDetectionStatusRequest = new CycleDetectionStatus (m_machine);
      var cycleDetectionStatus = Lemoine.Business.ServiceProvider
        .Get (cycleDetectionStatusRequest);
      var operationDetectionStatus = GetOperationDetectionStatus ();

      return cycleDetectionStatus.HasValue && operationDetectionStatus.HasValue
        && (Bound.Compare<DateTime> (m_range.Upper, cycleDetectionStatus.Value) < 0)
        && (Bound.Compare<DateTime> (m_range.Upper, operationDetectionStatus.Value) < 0);
    }

    DateTime? GetOperationDetectionStatus ()
    {
      if (m_operationDetectionStatusCache.HasValue) {
        return m_operationDetectionStatusCache.Value;
      }

      var operationDetectionStatusRequest = new OperationDetectionStatus (m_machine);
      m_operationDetectionStatusCache = Lemoine.Business.ServiceProvider
        .Get (operationDetectionStatusRequest);
      return m_operationDetectionStatusCache;
    }

    int GetNbPiecesFromCycles (CycleCounterValue cycleCounterValue)
    {
      if (cycleCounterValue.Operation is null) {
        if (log.IsInfoEnabled) {
          log.Info ($"GetNbPiecesFromCycles: operation is null, return default to 1");
        }
        return 1;
      }
      else { // cycleCounterValue.Operation is not null
        var operationCyclePropertiesRequest = new OperationCycleProperties (m_machine, cycleCounterValue.Operation, cycleCounterValue.ManufacturingOrder);
        var operationCycleProperties = ServiceProvider
          .Get (operationCyclePropertiesRequest);
        var nbPiecesByCycle = operationCycleProperties.NbPiecesByCycle;
        return (cycleCounterValue.TotalCycles - cycleCounterValue.AdjustedCycles) * nbPiecesByCycle + cycleCounterValue.AdjustedQuantity;
      }
    }

    IEnumerable<IIntermediateWorkPiece> GetIntermediateWorkPieces (IOperation operation)
    {
      var request = new Lemoine.Business.Operation.OperationIntermediateWorkPieces (operation);
      return Lemoine.Business.ServiceProvider.Get (request);
    }

    double? GetTarget (IOperation operation, TimeSpan productionDuration)
    {
      var intermediateWorkPieces = GetIntermediateWorkPieces (operation);
      var targets = intermediateWorkPieces
        .Select (x => GetTarget (x, productionDuration))
        .Where (x => x.HasValue);
      if (targets.Any ()) {
        var result = targets.Sum (x => x.Value);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTarget: return {result} for operation {((IDataWithId)operation).Id} and duration {productionDuration}");
        }
        return result;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTarget: no target for operation {((IDataWithId)operation).Id}");
        }
        return null;
      }
    }

    double? GetTarget (IIntermediateWorkPiece intermediateWorkPiece, TimeSpan productionDuration)
    {
      Debug.Assert (null != m_monitoredMachine);

      var standardProductionTargetPerHourRequest = new StandardProductionTargetPerHour (m_monitoredMachine, intermediateWorkPiece);
      var standardProductionTargetPerHour = Lemoine.Business.ServiceProvider
        .Get (standardProductionTargetPerHourRequest);
      if (!standardProductionTargetPerHour.HasValue) {
        return null;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTarget: target per hour={standardProductionTargetPerHour} for intermediate work piece {((IDataWithId)intermediateWorkPiece).Id}");
        }
        return standardProductionTargetPerHour.Value * productionDuration.TotalHours;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Operation.PartProductionRange.{m_machine.Id}.{m_range}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (PartProductionRangeResponse data)
    {
      if (m_range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeout: empty range");
        }
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }

      if (m_range.ContainsElement (DateTime.UtcNow)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeout: present");
        }
        return CacheTimeOut.CurrentLong.GetTimeSpan ();
      }
      else if (data.InProgress) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeout: data in progress");
        }
        return Lemoine.Info.ConfigSet
          .LoadAndGet (IN_PROGRESS_CACHE_TIME_OUT_KEY, IN_PROGRESS_CACHE_TIME_OUT_DEFAULT);
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeout: past");
        }
        return CacheTimeOut.OldLong.GetTimeSpan ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<PartProductionRangeResponse> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
