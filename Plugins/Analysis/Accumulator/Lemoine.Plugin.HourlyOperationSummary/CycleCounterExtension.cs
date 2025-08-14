// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business.Operation;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  public class CycleCounterExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICycleCounterExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CycleCounterExtension).FullName);

    IMonitoredMachine m_machine;

    UtcDateTimeRange m_preLoadRange = null;
    IEnumerable<IHourlyOperationSummary> m_preLoadData = null;

    public double Score => 200.0;

    public async Task<IEnumerable<CycleCounterValue>> GetNumberOfCyclesAsync (UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null)
    {
      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfCyclesAsync: empty range");
        }
        return new List<CycleCounterValue> ();
      }

      if ((null != preLoadRange) && (!preLoadRange.ContainsRange (range))) {
        log.Fatal ($"GetNumberOfCyclesAsync: invalid argument combination range {range} VS preLoadRange {preLoadRange} => de-active preLoadRange");
        Debug.Assert (false);
        preLoadRange = null;
      }

      if (!IsHourRange (range)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNumberOfCyclesAsync: range {range} does not correspond to a full hour");
        }
        throw new ArgumentException ("Not an hour range", "range");
      }
      Debug.Assert (range.Lower.HasValue);

      IEnumerable<IHourlyOperationSummary> summaries =
        new List<IHourlyOperationSummary> ();
      var localRange = range.ToLocalTime ();

      var detectionDateTime = GetDetectionDateTime ();
      if (log.IsDebugEnabled) {
        log.Debug ($"GetNumberOfCyclesAsync: detection date/time is {detectionDateTime}");
      }
      if (!detectionDateTime.HasValue) {
        log.Warn ($"GetNumberOfCyclesAsync: detection date/time is unknown");
      }
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.HourlyOperationSummary.CycleCounter")) {
          var dao = new HourlyOperationSummaryDAO ();

          if ((null != preLoadRange)
            && ((null == m_preLoadRange) || !m_preLoadRange.ContainsRange (preLoadRange))) {
            var localPreLoadRange = preLoadRange.ToLocalTime ();
            m_preLoadData = await dao.FindByHourAsync (m_machine, localPreLoadRange);
            m_preLoadRange = preLoadRange;
          }

          IEnumerable<IHourlyOperationSummary> s;
          if ((null != m_preLoadRange) && m_preLoadRange.ContainsRange (preLoadRange)) {
            s = m_preLoadData;
          }
          else {
            s = await dao.FindByHourAsync (m_machine, localRange);
          }

          foreach (var localDateHour in GetLocalHourEnumerator (localRange)) {
            var data = s.Where (x => x.LocalDateHour.Equals (localDateHour));
            if (!data.Any ()) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetNumberOfCyclesAsync: no data was recorded for local date hour {localDateHour}, give up");
              }
              throw new Exception ($"No data for local date hour {localDateHour}");
            }
            summaries = summaries.Concat (data);
          }

          var result = summaries
            .GroupBy (x => (operation: x.Operation, manufacturingOrder: x.ManufacturingOrder))
            .Select (x => new CycleCounterValue (x.Key.operation, x.Key.manufacturingOrder, GetTotalDuration (x, x.Key.operation, x.Key.manufacturingOrder, range), x.Sum (y => y.TotalCycles), x.Sum (y => y.AdjustedCycles), x.Sum (y => y.AdjustedQuantity), x.OrderByDescending (y => y.LocalDateHour).Any (y => IsInProgress (y, detectionDateTime))));
          if (log.IsDebugEnabled) {
            log.Debug ($"GetNumberOfCyclesAsync: return result for range {range}");
          }
          return result.ToList (); // To initialize all the data in this session and not later (IEnumerable is lazy)
        }
      }
    }

    public async Task<IEnumerable<CycleCounterValue>> GetNumberOfCyclesAsync (DateTime day, IShift shift)
    {
      var detectionDateTime = GetDetectionDateTime ();
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dao = new HourlyOperationSummaryDAO ();
        var data = await dao.FindByDayShiftAsync (m_machine, day, shift);
        if (!data.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetNumberOfCyclesAsync: no data was recorded for day {day} shift {shift}, give up");
          }
          throw new Exception ($"No data for local day {day} shift {shift}");
        }
        var result = data
          .GroupBy (x => (operation: x.Operation, manufacturingOrder: x.ManufacturingOrder))
          .Select (x => new CycleCounterValue (x.Key.operation, x.Key.manufacturingOrder, GetTotalDuration (x, x.Key.operation, x.Key.manufacturingOrder, day, shift), x.Sum (y => y.TotalCycles), x.Sum (y => y.AdjustedCycles), x.Sum (y => y.AdjustedQuantity), x.Any (y => IsInProgress (y, detectionDateTime))));
        return result;
      }
    }

    TimeSpan GetTotalDuration (IEnumerable<IHourlyOperationSummary> summaries, IOperation operation, IManufacturingOrder manufacturingOrder, DateTime day, IShift shift)
    {
      try {
        return TimeSpan.FromSeconds (summaries.Sum (x => GetEffectiveDuration (x).TotalSeconds));
      }
      catch (Exception) {
      }

      var localNow = DateTime.Now;
      var currentLocalDateHour = new DateTime (localNow.Year, localNow.Month, localNow.Day, localNow.Hour, 00, 00, DateTimeKind.Local);
      var pastSummaries = summaries
        .Where (s => s.LocalDateHour < currentLocalDateHour);
      var pastEffectiveDuration = TimeSpan.FromSeconds (pastSummaries.Sum (x => x.Duration.TotalSeconds));

      if (summaries.All (s => s.LocalDateHour < currentLocalDateHour)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTotalDuration: only past summaries");
        }
        return pastEffectiveDuration;
      }


      var presentRange = new UtcDateTimeRange (currentLocalDateHour, localNow);
      if (presentRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTotalDuration: presentRange {presentRange} is empty => return pastEffectiveDuration");
        }
        return pastEffectiveDuration;
      }

      TimeSpan presentEffectiveDuration;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.HourlyOperationSummary.CycleCounter.GetTotalDuration")) {
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindByDayShift (m_machine, day, shift)
            .Where (s => (((IDataWithId)operation)?.Id == ((IDataWithId)s.Operation)?.Id) && (((IDataWithId)manufacturingOrder)?.Id == ((IDataWithId)s.ManufacturingOrder)?.Id));
          var operationRanges = operationSlots
            .Select (s => new UtcDateTimeRange (presentRange.Intersects (s.DateTimeRange)))
            .Where (r => !r.IsEmpty ());
          Debug.Assert (operationRanges.All (r => r.Duration.HasValue));
          if (operationRanges.Any ()) {
            presentEffectiveDuration = TimeSpan.FromSeconds (operationRanges.Sum (x => x.Duration.Value.TotalSeconds));
          }
          else {
            presentEffectiveDuration = TimeSpan.FromTicks (0);
          }
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetTotalDuration: for day {day} shift {shift}, past effective duration={pastEffectiveDuration} present effective duration={presentEffectiveDuration}");
      }

      return pastEffectiveDuration.Add (presentEffectiveDuration);
    }

    TimeSpan GetTotalDuration (IEnumerable<IHourlyOperationSummary> summaries, IOperation operation, IManufacturingOrder manufacturingOrder, UtcDateTimeRange range)
    {
      try {
        var directResult = TimeSpan.FromSeconds (summaries.Sum (x => GetEffectiveDuration (x).TotalSeconds));
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTotalDuration: direct result {directResult} for range {range}");
        }
        return directResult;
      }
      catch (Exception) {
      }

      var localNow = DateTime.Now;
      var currentLocalDateHour = new DateTime (localNow.Year, localNow.Month, localNow.Day, localNow.Hour, 00, 00, DateTimeKind.Local);
      var pastSummaries = summaries
        .Where (s => s.LocalDateHour < currentLocalDateHour);
      var pastEffectiveDuration = TimeSpan.FromSeconds (pastSummaries.Sum (x => x.Duration.TotalSeconds));

      if (summaries.All (s => s.LocalDateHour < currentLocalDateHour)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTotalDuration: only past summaries");
        }
        return pastEffectiveDuration;
      }

      var presentRange = new UtcDateTimeRange (range
        .Intersects (new UtcDateTimeRange (currentLocalDateHour, localNow)));
      if (presentRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTotalDuration: presentRange {presentRange} is empty => return pastEffectiveDuration");
        }
        return pastEffectiveDuration;
      }

      TimeSpan presentEffectiveDuration;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.HourlyOperationSummary.CycleCounter.GetTotalDuration")) {
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRange (m_machine, presentRange)
            .Where (s => (((IDataWithId)operation)?.Id == ((IDataWithId)s.Operation)?.Id) && (((IDataWithId)manufacturingOrder)?.Id == ((IDataWithId)s.ManufacturingOrder)?.Id));
          var operationRanges = operationSlots
            .Select (s => new UtcDateTimeRange (presentRange.Intersects (s.DateTimeRange)))
            .Where (r => !r.IsEmpty ());
          Debug.Assert (operationRanges.All (r => r.Duration.HasValue));
          if (operationRanges.Any ()) {
            presentEffectiveDuration = TimeSpan.FromSeconds (operationRanges.Sum (x => x.Duration.Value.TotalSeconds));
          }
          else {
            presentEffectiveDuration = TimeSpan.FromTicks (0);
          }
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetTotalDuration: for range {range}, past effective duration={pastEffectiveDuration} present effective duration={presentEffectiveDuration}");
      }

      return pastEffectiveDuration.Add (presentEffectiveDuration);
    }

    TimeSpan GetEffectiveDuration (IHourlyOperationSummary summary)
    {
      var utcNow = DateTime.UtcNow;
      var fullRange = new UtcDateTimeRange (summary.LocalDateHour, summary.LocalDateHour.AddHours (1));
      if (Bound.Compare<DateTime> (fullRange.Upper, utcNow) <= 0) { // In the past
        return summary.Duration;
      }
      if (Bound.Compare<DateTime> (utcNow, fullRange.Lower) <= 0) { // In the future
        return TimeSpan.FromTicks (0);
      }
      Debug.Assert (fullRange.Duration.HasValue);
      var restrictedRange = new UtcDateTimeRange (fullRange.Lower, utcNow);
      if (fullRange.Duration.Value.Equals (summary.Duration)) { // Full period, restrict the duration to now
        return restrictedRange.Duration.Value;
      }
      var remainingRange = new UtcDateTimeRange (utcNow, fullRange.Upper);
      Debug.Assert (remainingRange.Duration.HasValue);
      if (summary.Duration < remainingRange.Duration.Value) { // This can be an operation that continues in the future, else its duration would be longer than remainingRange.Duration
        if (log.IsDebugEnabled) {
          log.Debug ($"GetEffectiveDuration: summary duration {summary.Duration} is shorter than the remaining hour duration => this does not correspond to a period that continues in the future and this is a real duration");
        }
        return summary.Duration;
      }

      throw new Exception ("The effective duration could not be guessed from the summary");
    }

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

    bool IsHourRange (UtcDateTimeRange range)
    {
      if (!range.Lower.HasValue || !range.Upper.HasValue) {
        return false;
      }

      if (!range.LowerInclusive || range.UpperInclusive) {
        return false;
      }

      var localRange = range.ToLocalTime ();
      Debug.Assert (localRange.Lower.HasValue);
      Debug.Assert (localRange.Upper.HasValue);

      var localLower = localRange.Lower.Value;
      var localUpper = localRange.Upper.Value;
      return (0 == localLower.Minute) && (0 == localLower.Second) && (0 == localLower.Millisecond)
        && (0 == localUpper.Minute) && (0 == localUpper.Second) && (0 == localUpper.Millisecond);
    }

    bool IsInProgress (IHourlyOperationSummary summary, DateTime? detectionDateTime)
    {
      Debug.Assert (null != summary);

      if (!detectionDateTime.HasValue) {
        log.Error ("IsInProgress: detection date/time is not known => return false");
        return false;
      }

      var localUpper = summary.LocalDateHour.AddHours (1);
      var utcUpper = localUpper.ToUniversalTime ();
      return detectionDateTime.Value <= utcUpper;
    }

    DateTime? GetDetectionDateTime ()
    {
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

      return operationDetectionStatus.Value < cycleDetectionStatus.Value
        ? operationDetectionStatus.Value
        : cycleDetectionStatus.Value;
    }

    IEnumerable<DateTime> GetLocalHourEnumerator (LocalDateTimeRange localRange)
    {
      Debug.Assert (!localRange.IsEmpty ());
      Debug.Assert (localRange.Lower.HasValue);
      Debug.Assert (localRange.Upper.HasValue);

      var localDateHour = localRange.Lower.Value;
      while (localDateHour < localRange.Upper.Value) {
        yield return localDateHour;
        localDateHour = localDateHour.AddHours (1);
      }
    }
  }
}
