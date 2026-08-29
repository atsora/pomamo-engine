// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Request class to get the machining duration of a machine in a range: the time it was
  /// in a running machine mode
  ///
  /// Unlike <see cref="RunningDuration"/>, which only returns the duration, the response
  /// also says up to when the duration could be counted when the range goes past what the
  /// activity analysis has processed. A caller that accumulates the machining duration of
  /// a growing period needs that date/time to know what is final and what is not
  /// </summary>
  public sealed class MachiningDuration
    : IRequest<MachiningDurationResponse>
  {
    static readonly string CACHE_TIMEOUT_OLD_KEY = "Business.MachineMode.MachiningDuration.CacheTimeOut.Old";
    static readonly TimeSpan CACHE_TIMEOUT_OLD_DEFAULT = TimeSpan.FromHours (3);
    static readonly string CACHE_TIMEOUT_PAST_KEY = "Business.MachineMode.MachiningDuration.CacheTimeOut.Past";
    static readonly TimeSpan CACHE_TIMEOUT_PAST_DEFAULT = CacheTimeOut.PastShort.GetTimeSpan ();
    static readonly string CACHE_TIMEOUT_CURRENT_KEY = "Business.MachineMode.MachiningDuration.CacheTimeOut.Current";
    static readonly TimeSpan CACHE_TIMEOUT_CURRENT_DEFAULT = CacheTimeOut.CurrentShort.GetTimeSpan ();

    static readonly ILog log = LogManager.GetLogger (typeof (MachiningDuration).FullName);

    /// <summary>
    /// Machine (not null)
    /// </summary>
    IMachine Machine { get; set; }

    /// <summary>
    /// Range (not empty)
    /// </summary>
    UtcDateTimeRange Range { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not empty</param>
    public MachiningDuration (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      Debug.Assert (!range.IsEmpty ());

      this.Machine = machine;
      this.Range = range;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public MachiningDurationResponse Get ()
    {
      if (this.Range.IsEmpty ()) {
        log.Warn ($"Get: specified range is empty => return 0s");
        return new MachiningDurationResponse (TimeSpan.FromSeconds (0), null);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Read-write because of the day processing
        using (var transaction = session.BeginTransaction ("Business.MachineMode.MachiningDuration")) {
          var upperBound = Bound.GetMinimum<DateTime> (DateTime.UtcNow, this.Range.Upper).Value;
          var upperDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAt (upperBound);
          MachiningDurationResponse result;
          if (IsUpperInsideDay (upperBound, upperDaySlot)) {
            var pastRange = GetPastRange (upperDaySlot);
            var past = (pastRange is null)
              ? null
              : ServiceProvider.Get (new MachiningDuration (this.Machine, pastRange));
            result = ComputeInsideDay (upperDaySlot, past);
          }
          else {
            result = ComputeFullDays (upperBound, GetUpperFullDay (upperBound, upperDaySlot));
          }
          transaction.Commit ();
          return result;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public async Task<MachiningDurationResponse> GetAsync ()
    {
      if (this.Range.IsEmpty ()) {
        log.Warn ($"GetAsync: specified range is empty => return 0s");
        return new MachiningDurationResponse (TimeSpan.FromSeconds (0), null);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Read-write because of the day processing
        using (var transaction = session.BeginTransaction ("Business.MachineMode.MachiningDuration")) {
          var upperBound = Bound.GetMinimum<DateTime> (DateTime.UtcNow, this.Range.Upper).Value;
          var upperDaySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAtAsync (upperBound);
          MachiningDurationResponse result;
          if (IsUpperInsideDay (upperBound, upperDaySlot)) {
            var pastRange = GetPastRange (upperDaySlot);
            var past = (pastRange is null)
              ? null
              : await ServiceProvider.GetAsync (new MachiningDuration (this.Machine, pastRange));
            result = await ComputeInsideDayAsync (upperDaySlot, past);
          }
          else {
            result = await ComputeFullDaysAsync (upperBound, GetUpperFullDay (upperBound, upperDaySlot));
          }
          transaction.Commit ();
          return result;
        }
      }
    }

    /// <summary>
    /// Is the upper bound strictly inside the day it belongs to?
    ///
    /// A day slot that is missing, or that carries no day, is a fallback case: the whole
    /// period is then counted with the full days, like RunningDuration used to do
    /// </summary>
    /// <param name="upperBound"></param>
    /// <param name="upperDaySlot">may be null</param>
    /// <returns></returns>
    bool IsUpperInsideDay (DateTime upperBound, IDaySlot upperDaySlot)
    {
      if (upperDaySlot is null) {
        log.Error ($"IsUpperInsideDay: no processed day at {upperBound} => fallback, return an approximative value");
        Debug.Assert (false);
        return false;
      }
      if (!upperDaySlot.Day.HasValue) {
        log.Error ($"IsUpperInsideDay: day slot has no associated day => fallback, return an approximative value");
        Debug.Assert (false);
        return false;
      }
      return !Bound<DateTime>.Equals (upperBound, upperDaySlot.DateTimeRange.Lower);
    }

    /// <summary>
    /// Get the range that is before the day the upper bound belongs to
    /// </summary>
    /// <param name="upperDaySlot">not null, with a day</param>
    /// <returns>null if the requested range does not start before that day</returns>
    UtcDateTimeRange GetPastRange (IDaySlot upperDaySlot)
    {
      if (0 <= Bound.Compare<DateTime> (this.Range.Lower, upperDaySlot.DateTimeRange.Lower)) {
        return null;
      }
      Debug.Assert (upperDaySlot.DateTimeRange.Lower.HasValue);
      var pastRange = new UtcDateTimeRange (this.Range.Lower, upperDaySlot.DateTimeRange.Lower.Value);
      Debug.Assert (!pastRange.IsEmpty ());
      return pastRange;
    }

    /// <summary>
    /// Count the machining duration when the upper bound is inside a day: the days before
    /// it come from the past response, the day itself from the reason slots
    /// </summary>
    /// <param name="upperDaySlot">not null, with a day</param>
    /// <param name="past">machining duration of the days before, null if there is none</param>
    /// <returns>not null</returns>
    MachiningDurationResponse ComputeInsideDay (IDaySlot upperDaySlot, MachiningDurationResponse past)
    {
      var currentRange = new UtcDateTimeRange (upperDaySlot.DateTimeRange.Intersects (this.Range));
      var current = SumRunningReasonSlots (currentRange);
      var duration = (past is null)
        ? current.Duration
        : past.Duration.Add (current.Duration);
      // When the data already stopped before the day of the upper bound, that is where it
      // stops: the reason slots of the day carry nothing after it
      var countedUntil = past?.MaxDateTime ?? current.CountedUntil;
      return BuildResponse (duration, countedUntil);
    }

    /// <summary>
    /// Count the machining duration when the upper bound is inside a day, asynchronously
    /// </summary>
    /// <param name="upperDaySlot">not null, with a day</param>
    /// <param name="past">machining duration of the days before, null if there is none</param>
    /// <returns>not null</returns>
    async Task<MachiningDurationResponse> ComputeInsideDayAsync (IDaySlot upperDaySlot, MachiningDurationResponse past)
    {
      var currentRange = new UtcDateTimeRange (upperDaySlot.DateTimeRange.Intersects (this.Range));
      var current = await SumRunningReasonSlotsAsync (currentRange);
      var duration = (past is null)
        ? current.Duration
        : past.Duration.Add (current.Duration);
      // When the data already stopped before the day of the upper bound, that is where it
      // stops: the reason slots of the day carry nothing after it
      var countedUntil = past?.MaxDateTime ?? current.CountedUntil;
      return BuildResponse (duration, countedUntil);
    }

    /// <summary>
    /// Count the machining duration from the full days, plus the part of the first day the
    /// requested range starts inside
    /// </summary>
    /// <param name="upperBound"></param>
    /// <param name="upperFullDay"></param>
    /// <returns>not null</returns>
    MachiningDurationResponse ComputeFullDays (DateTime upperBound, UpperBound<DateTime> upperFullDay)
    {
      LowerBound<DateTime> lowerFullDay;
      var duration = TimeSpan.FromSeconds (0);

      if (this.Range.Lower.HasValue) {
        var lowerDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedAt (this.Range.Lower.Value);
        if (lowerDaySlot is null) {
          log.Error ($"ComputeFullDays: no processed day at {this.Range.Lower.Value} => fallback, return an approximative value");
          Debug.Assert (false);
          lowerFullDay = this.Range.Lower.Value.Date;
        }
        else if (!lowerDaySlot.Day.HasValue) {
          log.Error ($"ComputeFullDays: day slot has no associated day => fallback, return an approximative value");
          Debug.Assert (false);
          lowerFullDay = this.Range.Lower.Value.Date;
        }
        else if (!Bound<DateTime>.Equals (this.Range.Lower, lowerDaySlot.DateTimeRange.Lower)) {
          // Count the period [this.Range.Lower, lowerDaySlot.DateTimeRange.Upper)
          Debug.Assert (lowerDaySlot.DateTimeRange.Upper.HasValue);
          Debug.Assert (this.Range.Lower.Value < lowerDaySlot.DateTimeRange.Upper.Value);
          var firstDayRange = new UtcDateTimeRange (this.Range.Lower.Value, lowerDaySlot.DateTimeRange.Upper.Value);
          duration = duration.Add (SumRunningReasonSlots (firstDayRange).Duration);
          lowerFullDay = lowerDaySlot.Day.Value.AddDays (1);
        }
        else {
          lowerFullDay = lowerDaySlot.Day.Value;
        }
      }
      else {
        lowerFullDay = new LowerBound<DateTime> (null);
      }

      if (lowerFullDay < upperFullDay) {
        var dayRange = new DayRange (lowerFullDay, upperFullDay);
        var summaries = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
          .FindInDayRangeWithMachineMode (this.Machine, dayRange);
        var runningSeconds = summaries
          .Where (s => s.MachineMode.Running.HasValue && s.MachineMode.Running.Value)
          .Sum (s => s.Time.TotalSeconds);
        duration = duration.Add (TimeSpan.FromSeconds (runningSeconds));
      }

      // The upper bound falls on a day boundary: the days that are counted are complete
      return BuildResponse (duration, upperBound);
    }

    /// <summary>
    /// Count the machining duration from the full days, plus the part of the first day the
    /// requested range starts inside, asynchronously
    /// </summary>
    /// <param name="upperBound"></param>
    /// <param name="upperFullDay"></param>
    /// <returns>not null</returns>
    async Task<MachiningDurationResponse> ComputeFullDaysAsync (DateTime upperBound, UpperBound<DateTime> upperFullDay)
    {
      LowerBound<DateTime> lowerFullDay;
      var duration = TimeSpan.FromSeconds (0);

      if (this.Range.Lower.HasValue) {
        var lowerDaySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedAtAsync (this.Range.Lower.Value);
        if (lowerDaySlot is null) {
          log.Error ($"ComputeFullDaysAsync: no processed day at {this.Range.Lower.Value} => fallback, return an approximative value");
          Debug.Assert (false);
          lowerFullDay = this.Range.Lower.Value.Date;
        }
        else if (!lowerDaySlot.Day.HasValue) {
          log.Error ($"ComputeFullDaysAsync: day slot has no associated day => fallback, return an approximative value");
          Debug.Assert (false);
          lowerFullDay = this.Range.Lower.Value.Date;
        }
        else if (!Bound<DateTime>.Equals (this.Range.Lower, lowerDaySlot.DateTimeRange.Lower)) {
          // Count the period [this.Range.Lower, lowerDaySlot.DateTimeRange.Upper)
          Debug.Assert (lowerDaySlot.DateTimeRange.Upper.HasValue);
          Debug.Assert (this.Range.Lower.Value < lowerDaySlot.DateTimeRange.Upper.Value);
          var firstDayRange = new UtcDateTimeRange (this.Range.Lower.Value, lowerDaySlot.DateTimeRange.Upper.Value);
          duration = duration.Add ((await SumRunningReasonSlotsAsync (firstDayRange)).Duration);
          lowerFullDay = lowerDaySlot.Day.Value.AddDays (1);
        }
        else {
          lowerFullDay = lowerDaySlot.Day.Value;
        }
      }
      else {
        lowerFullDay = new LowerBound<DateTime> (null);
      }

      if (lowerFullDay < upperFullDay) {
        var dayRange = new DayRange (lowerFullDay, upperFullDay);
        var summaries = await ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
          .FindInDayRangeWithMachineModeAsync (this.Machine, dayRange);
        var runningSeconds = summaries
          .Where (s => s.MachineMode.Running.HasValue && s.MachineMode.Running.Value)
          .Sum (s => s.Time.TotalSeconds);
        duration = duration.Add (TimeSpan.FromSeconds (runningSeconds));
      }

      // The upper bound falls on a day boundary: the days that are counted are complete
      return BuildResponse (duration, upperBound);
    }

    /// <summary>
    /// Get the last full day to count from the summaries
    /// </summary>
    /// <param name="upperBound"></param>
    /// <param name="upperDaySlot">may be null</param>
    /// <returns></returns>
    UpperBound<DateTime> GetUpperFullDay (DateTime upperBound, IDaySlot upperDaySlot)
    {
      if (upperDaySlot is null || !upperDaySlot.Day.HasValue) {
        return upperBound.Date;
      }
      return upperDaySlot.Day.Value.AddDays (-1);
    }

    /// <summary>
    /// Sum the duration of the running reason slots of a range, and get the date/time the
    /// reason slots go up to
    /// </summary>
    /// <param name="range">not empty</param>
    /// <returns>the machining duration, and the date/time the reason slots stop at</returns>
    (TimeSpan Duration, DateTime CountedUntil) SumRunningReasonSlots (UtcDateTimeRange range)
    {
      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAllInUtcRangeWithMachineMode (this.Machine, range);
      return SumRunningReasonSlots (reasonSlots, range);
    }

    /// <summary>
    /// Sum the duration of the running reason slots that were read, and get the date/time
    /// they go up to
    /// </summary>
    /// <param name="reasonSlots">not null</param>
    /// <param name="range">not empty</param>
    /// <returns>the machining duration, and the date/time the reason slots stop at</returns>
    static (TimeSpan Duration, DateTime CountedUntil) SumRunningReasonSlots (IEnumerable<IReasonSlot> reasonSlots, UtcDateTimeRange range)
    {
      var runningSeconds = reasonSlots
        .Where (s => s.Running)
        .Where (s => s.Duration.HasValue)
        .Sum (s => s.Duration.Value.TotalSeconds);

      // The reason slots are contiguous where the activity was analysed: the end of the
      // last one is where the data stops
      var countedUntil = range.Lower.HasValue ? range.Lower.Value : DateTime.MinValue;
      foreach (var reasonSlot in reasonSlots) {
        var end = reasonSlot.DateTimeRange.Upper.HasValue
          ? reasonSlot.DateTimeRange.Upper.Value
          : DateTime.MaxValue;
        if (range.Upper.HasValue && (range.Upper.Value < end)) {
          end = range.Upper.Value;
        }
        if (countedUntil < end) {
          countedUntil = end;
        }
      }
      return (TimeSpan.FromSeconds (runningSeconds), countedUntil);
    }

    /// <summary>
    /// Sum the duration of the running reason slots of a range, and get the date/time the
    /// reason slots go up to, asynchronously
    /// </summary>
    /// <param name="range">not empty</param>
    /// <returns>the machining duration, and the date/time the reason slots stop at</returns>
    async Task<(TimeSpan Duration, DateTime CountedUntil)> SumRunningReasonSlotsAsync (UtcDateTimeRange range)
    {
      var reasonSlots = await ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAllInUtcRangeWithMachineModeAsync (this.Machine, range);
      return SumRunningReasonSlots (reasonSlots, range);
    }

    /// <summary>
    /// Build the response: the date/time the duration was counted up to is only reported
    /// when the whole requested range could not be counted
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="countedUntil"></param>
    /// <returns>not null</returns>
    MachiningDurationResponse BuildResponse (TimeSpan duration, DateTime countedUntil)
    {
      if (this.Range.Upper.HasValue && (this.Range.Upper.Value <= countedUntil)) {
        return new MachiningDurationResponse (duration, null);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"BuildResponse: machine {this.Machine.Id} range {this.Range} counted up to {countedUntil} only");
      }
      return new MachiningDurationResponse (duration, countedUntil);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.MachineMode.MachiningDuration." + this.Machine.Id + "."
        + this.Range.ToString (dt => dt.ToString ("yyyy-MM-ddTHH:mm:ss"));
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<MachiningDurationResponse> data) => true;

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (MachiningDurationResponse data)
    {
      if (this.Range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        var daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        return this.Range.IsStrictlyLeftOf (daySlot.DateTimeRange)
          ? Lemoine.Info.ConfigSet.LoadAndGet (CACHE_TIMEOUT_OLD_KEY, CACHE_TIMEOUT_OLD_DEFAULT)
          : Lemoine.Info.ConfigSet.LoadAndGet (CACHE_TIMEOUT_PAST_KEY, CACHE_TIMEOUT_PAST_DEFAULT);
      }
      else { // Current or future
        return Lemoine.Info.ConfigSet.LoadAndGet (CACHE_TIMEOUT_CURRENT_KEY, CACHE_TIMEOUT_CURRENT_DEFAULT);
      }
    }
  }
}
