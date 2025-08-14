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

namespace Lemoine.Plugin.DefaultCycleCounter
{
  public class CycleCounterExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICycleCounterExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CycleCounterExtension).FullName);

    IMonitoredMachine m_machine;

    (UtcDateTimeRange range, IEnumerable<IOperationCycle> data) m_preLoad = (null, null);
    (bool valid, DateTime? dateTime) m_detectionCache = (false, null);

    public double Score => 10.0;

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
          log.Debug ($"GetNumberOfPiecesAsync: empty range");
        }
        return new List<CycleCounterValue> ();
      }

      if (!range.Lower.HasValue) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfPiecesAsync: no lower bound in range {range}");
        }
        throw new ArgumentException ("No lower bound in range", "range");
      }

      if (System.DateTime.UtcNow <= range.Lower.Value) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfPiecesAsync: range {range} after now => return an empty list");
        }
        return new List<CycleCounterValue> ();
      }

      if ((null != preLoadRange) && (!preLoadRange.ContainsRange (range))) {
        log.Fatal ($"GetNumberOfCyclesAsync: invalid argument combination range {range} VS preLoadRange {preLoadRange} => de-activate preLoadRange");
        Debug.Assert (false);
        preLoadRange = null;
      }

      // DetectionDateTime: lazy
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.DefaultCycleCounter.CycleCounter")) {

          if ((null != preLoadRange)
          && ((null == m_preLoad.range) || !m_preLoad.range.ContainsRange (preLoadRange))) {
            m_preLoad = (preLoadRange, (await FindOperationCyclesAsync (preLoadRange)).ToList ());
          }

          IEnumerable<IOperationCycle> operationCycles;
          if ((null != m_preLoad.range) && m_preLoad.range.ContainsRange (preLoadRange)) {
            operationCycles = m_preLoad.data
              .Where (c => range.ContainsElement (c.DateTime));
          }
          else {
            operationCycles = await FindOperationCyclesAsync (range);
          }

          operationCycles = operationCycles.Where (c => c.Full);

          var withOperationSlot = operationCycles
            .Where (c => (null != c.OperationSlot))
            .GroupBy (c => (operation: c.OperationSlot.Operation, manufacturingOrder: c.OperationSlot.ManufacturingOrder))
            .Select (x => new CycleCounterValue (x.Key.operation, x.Key.manufacturingOrder, GetDuration (x, range), x.Count (), x.Count (c => c.Quantity.HasValue), x.Where (c => c.Quantity.HasValue).Sum (c => c.Quantity.Value), GetInProgress (x, range)));
          var withoutOperationSlot = operationCycles
            .Where (c => (null == c.OperationSlot))
            .Select (x => new CycleCounterValue (null, null, GetDuration (x), 1, x.Quantity.HasValue ? 1 : 0, x.Quantity.HasValue ? x.Quantity.Value : 0, GetInProgress (new List<IOperationCycle> { x }, range)));
          var result = withOperationSlot.Concat (withoutOperationSlot);
          return result.ToList (); // To initialize all the data in this session and not later (IEnumerable is lazy)
        }
      }
    }

    async Task<IEnumerable<IOperationCycle>> FindOperationCyclesAsync (UtcDateTimeRange range)
    {
      var operationCycles = await ModelDAOHelper.DAOFactory.OperationCycleDAO
        .FindOverlapsRangeAsync (m_machine, range);
      return operationCycles;
    }

    TimeSpan GetDuration (IEnumerable<IOperationCycle> operationCycles, UtcDateTimeRange range)
    {
      var seconds = operationCycles
        .GroupBy (x => x.OperationSlot)
        .Sum (x => GetEffectiveDuration (x.Key, range).TotalSeconds);
      if (log.IsDebugEnabled) {
        log.Debug ($"GetDuration: for a set of operation cycles with operation slot in range {range}, {seconds}s");
      }
      return TimeSpan.FromSeconds (seconds);
    }

    TimeSpan GetDuration (IOperationCycle operationCycleWithoutOperationSlot)
    {
      if (operationCycleWithoutOperationSlot.End.HasValue && operationCycleWithoutOperationSlot.Begin.HasValue) {
        return operationCycleWithoutOperationSlot.End.Value
          .Subtract (operationCycleWithoutOperationSlot.Begin.Value);
      }
      return TimeSpan.FromSeconds (0);
    }

    bool GetInProgress (IEnumerable<IOperationCycle> operationCycles, UtcDateTimeRange range)
    {
      var detectionDateTime = GetDetectionDateTime ();

      if (!detectionDateTime.HasValue) {
        log.Error ("GetInProgress: detection date/time is not known");
        return false;
      }

      if (Bound.Compare<DateTime> (range.Upper, detectionDateTime.Value) <= 0) {
        return false;
      }

      return operationCycles
        .GroupBy (x => x.OperationSlot?.Id ?? 0)
        .Any (x => IsInProgress (x.Select (y => y.OperationSlot).First (), detectionDateTime.Value));
    }

    bool IsInProgress (IOperationSlot operationSlot, DateTime detectionDateTime)
    {
      if (null == operationSlot) {
        return false;
      }

      return Bound.Compare<DateTime> (detectionDateTime, operationSlot.DateTimeRange.Upper) <= 0;
    }

    TimeSpan GetEffectiveDuration (IOperationSlot operationSlot, UtcDateTimeRange range)
    {
      Debug.Assert (operationSlot.DateTimeRange.Lower.HasValue);

      var intersection = new UtcDateTimeRange (operationSlot.DateTimeRange.Intersects (range));
      if (intersection.IsEmpty ()) {
        if (log.IsWarnEnabled) {
          log.Warn ($"GetEffectiveDuration: unexpected ? intersection between range={range} and operation slot {operationSlot.Id} with range {operationSlot.DateTimeRange} is empty => return 0:00:00");
        }
        return TimeSpan.FromTicks (0);
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"GetEffectiveDuration: consider intersection={intersection} for range={range} and operation slot {operationSlot.Id} with range {operationSlot.DateTimeRange}");
      }

      var now = System.DateTime.UtcNow;
      if (intersection.Upper.HasValue && intersection.Upper.Value < now) {
        Debug.Assert (intersection.Duration.HasValue);
        return intersection.Duration.Value;
      }
      else {
        Debug.Assert (intersection.Lower.HasValue);
        return now.Subtract (intersection.Lower.Value);
      }
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
