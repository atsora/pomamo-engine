// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business.Operation;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.OperationSlotByMachineShift
{
  public class CycleCounterExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICycleCounterExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CycleCounterExtension).FullName);

    static readonly string MIN_SHIFT_DURATION_KEY = "Plugin.ShortCyclesByMachineShift.MinShiftDuration";
    static readonly TimeSpan MIN_SHIFT_DURATION_DEFAULT = TimeSpan.FromHours (6);

    IMonitoredMachine m_machine;

    (UtcDateTimeRange range, IEnumerable<IOperationSlot> data) m_preLoad = (null, null);
    (bool valid, DateTime? dateTime) m_detectionCache = (false, null);

    public double Score => 300.0;

    public bool Initialize (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      if (null == machine) {
        log.Fatal ("Initialize: machine is null");
        throw new ArgumentNullException ("machine");
      }

      m_machine = machine;

      return true;
    }

    public async Task<IEnumerable<CycleCounterValue>> GetNumberOfCyclesAsync (UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null)
    {
      m_detectionCache = (false, null);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfCyclesAsync: empty range");
        }
        return new List<CycleCounterValue> ();
      }

      if (!range.Lower.HasValue) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfCyclesAsync: no lower bound in range {range}");
        }
        throw new ArgumentException ("No lower bound in range", "range");
      }

      if (System.DateTime.UtcNow <= range.Lower.Value) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfCyclesAsync: range {range} after now => return an empty list");
        }
        return new List<CycleCounterValue> ();
      }

      if ((null != preLoadRange) && (!preLoadRange.ContainsRange (range))) {
        log.Fatal ($"GetNumberOfCyclesAsync: invalid argument combination range {range} VS preLoadRange {preLoadRange} => de-activate preLoadRange");
        Debug.Assert (false);
        preLoadRange = null;
      }

      if (range.Upper.HasValue) {
        var rangeDuration = range.Duration.Value;
        var minShiftDuration = Lemoine.Info.ConfigSet
          .LoadAndGet (MIN_SHIFT_DURATION_KEY, MIN_SHIFT_DURATION_DEFAULT);
        if (rangeDuration < minShiftDuration) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetNumberOfCyclesAsync: range duration {rangeDuration} of range {range} is shorter than min shift duration {minShiftDuration} => return an exception");
          }
          throw new ArgumentException ("Range duration shorter than min shift duration", "range");
        }
      }

      // DetectionDateTime: lazy
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.OperationSlotByMachineShift.CycleCounter")) {
          IOperationSlot operationSlotAtStart = await GetOperationSlotAtStartAsync (range);
          if ((operationSlotAtStart is null) || !Bound.Equals<DateTime> (range.Lower, operationSlotAtStart.DateTimeRange.Lower)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetNumberOfCyclesAsync: operation slot at {range.Lower} starts at {operationSlotAtStart?.DateTimeRange?.Lower} => give up");
            }
            throw new Exception ($"Lower bound does not match an operation slot");
          }
          Debug.Assert (null != operationSlotAtStart);
          IOperationSlot operationSlotAtEnd = null;
          if (range.Upper.HasValue && (range.Upper.Value < System.DateTime.UtcNow)
            && !Bound.Equals<DateTime> (operationSlotAtStart.DateTimeRange.Upper, range.Upper)) {
            operationSlotAtEnd = GetOperationSlotWithEnd (range);
            if ((null == operationSlotAtEnd) || !Bound.Equals<DateTime> (range.Upper, operationSlotAtEnd.DateTimeRange.Upper)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetNumberOfCyclesAsync: no operation slot with end {range.Upper} => give up");
              }
              throw new Exception ($"Upper bound does not match an operation slot");
            }
          }

          if ((null != preLoadRange)
          && ((null == m_preLoad.range) || !m_preLoad.range.ContainsRange (preLoadRange))) {
            m_preLoad = (preLoadRange, (await FindOperationSlotsOverlapsRangeAsync (preLoadRange, operationSlotAtStart, operationSlotAtEnd)).ToList ());
          }

          var operationSlots = await FindOperationSlotsOverlapsRangeAsync (range, operationSlotAtStart, operationSlotAtEnd);
          var result = operationSlots
            .GroupBy (x => (operation: x.Operation, task: x.Task))
            .Select (x => new CycleCounterValue (x.Key.operation, x.Key.task, TimeSpan.FromSeconds (x.Sum (y => GetEffectiveDuration (y).TotalSeconds)), x.Sum (y => y.TotalCycles), x.Sum (y => y.AdjustedCycles), x.Sum (y => y.AdjustedQuantity), x.OrderByDescending (y => y.DateTimeRange).Any (y => IsInProgress (y))));
          return result.ToList (); // To initialize all the data in this session and not later (IEnumerable is lazy)
        }
      }
    }

    async Task<IOperationSlot> GetOperationSlotAtStartAsync (UtcDateTimeRange range)
    {
      var start = range.Lower.Value;

      if ((null != m_preLoad.range) && m_preLoad.range.ContainsElement (start)) {
        return m_preLoad.data
          .FirstOrDefault (s => s.DateTimeRange.ContainsElement (start));
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        return await ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAtAsync (m_machine, range.Lower);
      }
    }

    IOperationSlot GetOperationSlotWithEnd (UtcDateTimeRange range)
    {
      Debug.Assert (range.Upper.HasValue);

      var end = range.Upper.Value;

      if ((null != m_preLoad.range)
        && new UtcDateTimeRange (m_preLoad.range.Lower, m_preLoad.range.Upper, "(]")
        .ContainsElement (end)) {
        return m_preLoad.data
          .FirstOrDefault (s => Bound.Equals (s.DateTimeRange.Upper, end));
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        return ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindWithEnd (m_machine, range.Upper.Value);
      }
    }

    async Task<IEnumerable<IOperationSlot>> FindOperationSlotsOverlapsRangeAsync (UtcDateTimeRange range, IOperationSlot operationSlotAtStart, IOperationSlot operationSlotAtEnd)
    {
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (null != operationSlotAtStart);

      if ((null != m_preLoad.range) && m_preLoad.range.ContainsRange (range)) {
        return m_preLoad.data
          .Where (s => s.DateTimeRange.Overlaps (range));
      }

      IEnumerable<IOperationSlot> operationSlots = new List<IOperationSlot> { operationSlotAtStart };
      if (operationSlotAtStart.DateTimeRange.Upper.HasValue) {
        var newRange = new UtcDateTimeRange (operationSlotAtStart.DateTimeRange.Upper.Value, range.Upper, !operationSlotAtStart.DateTimeRange.UpperInclusive, range.UpperInclusive);
        if (!newRange.IsEmpty ()) {
          if (null != operationSlotAtEnd) {
            Debug.Assert (operationSlotAtEnd.DateTimeRange.Lower.HasValue);
            newRange = new UtcDateTimeRange (newRange.Lower, operationSlotAtEnd.DateTimeRange.Lower.Value, newRange.LowerInclusive, !operationSlotAtEnd.DateTimeRange.LowerInclusive);
          }
        }
        if (!newRange.IsEmpty ()) {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            var otherOperationSlots = await ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindOverlapsRangeAsync (m_machine, newRange);
            operationSlots = operationSlots.Concat (otherOperationSlots);
          }
        }
        if (null != operationSlotAtEnd) {
          operationSlots = operationSlots.Concat (new List<IOperationSlot> { operationSlotAtEnd});
        }
      }

      return operationSlots;
    }

    TimeSpan GetEffectiveDuration (IOperationSlot operationSlot)
    {
      Debug.Assert (operationSlot.DateTimeRange.Lower.HasValue);

      var now = System.DateTime.UtcNow;
      if (operationSlot.DateTimeRange.Upper.HasValue && operationSlot.DateTimeRange.Upper.Value < now) {
        return operationSlot.DateTimeRange.Duration.Value;
      }
      else {
        return now.Subtract (operationSlot.DateTimeRange.Lower.Value);
      }
    }

    bool IsInProgress (IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationSlot);

      var detectionDateTime = GetDetectionDateTime ();
      if (!detectionDateTime.HasValue) {
        log.Error ("IsInProgress: detection date/time is not known");
        return false;
      }

      return Bound.Compare<DateTime> (detectionDateTime.Value, operationSlot.DateTimeRange.Upper) <= 0;
    }

    DateTime? GetDetectionDateTime ()
    {
      if (m_detectionCache.valid) {
        return m_detectionCache.dateTime; // Cache
      }

      var cycleDetectionStatusRequest = new CycleDetectionStatus (m_machine);
      var cycleDetectionStatus = Lemoine.Business.ServiceProvider
        .Get (cycleDetectionStatusRequest);
      if (!cycleDetectionStatus.HasValue) {
        return null;
      }

      var operationDetectionStatusRequest = new OperationDetectionStatus (m_machine);
      var operationDetectionStatus = Lemoine.Business.ServiceProvider
        .Get (operationDetectionStatusRequest);
      if (!operationDetectionStatus.HasValue) {
        return null;
      }

      m_detectionCache = (true, operationDetectionStatus.Value < cycleDetectionStatus.Value
        ? operationDetectionStatus.Value
        : cycleDetectionStatus.Value);
      return m_detectionCache.dateTime;
    }
  }
}
