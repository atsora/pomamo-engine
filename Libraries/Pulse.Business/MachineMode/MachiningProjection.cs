// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Period of time with the utilization rate the machine is expected to machine at
  /// </summary>
  [Serializable]
  public class MachiningRateSegment
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC, null when the period has no end</param>
    /// <param name="rate">utilization rate between 0 and 1, null when it is not known</param>
    public MachiningRateSegment (DateTime begin, DateTime? end, double? rate)
    {
      this.Begin = begin;
      this.End = end;
      this.Rate = rate;
    }

    /// <summary>
    /// Begin of the period, in UTC
    /// </summary>
    public DateTime Begin { get; }

    /// <summary>
    /// End of the period, in UTC. Null when the period has no end
    /// </summary>
    public DateTime? End { get; }

    /// <summary>
    /// Utilization rate in the period, between 0 and 1
    ///
    /// Null when it is not known: the default rate of the projection applies then
    /// </summary>
    public double? Rate { get; }
  }

  /// <summary>
  /// Projection of the machining time of a machine on the time axis: in how much time it
  /// machines a given number of seconds
  ///
  /// The future is cut into periods with their own utilization rate, the rate of the machine
  /// observation state that is planned in each of them. Where nothing is planned, or where
  /// the rate of the planned state is not known, a default rate applies
  /// </summary>
  [Serializable]
  public class MachiningProjection
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachiningProjection).FullName);

    readonly IReadOnlyList<MachiningRateSegment> m_segments;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="segments">periods with their rate, not null, not overlapping</param>
    /// <param name="defaultRate">rate between and after the periods, or when the rate of a period is not known. May be null</param>
    public MachiningProjection (IEnumerable<MachiningRateSegment> segments, double? defaultRate)
    {
      if (segments is null) {
        log.Fatal ("MachiningProjection: segments is null");
        throw new ArgumentNullException (nameof (segments));
      }

      m_segments = segments
        .OrderBy (s => s.Begin)
        .ToList ();
      this.DefaultRate = defaultRate;
    }

    /// <summary>
    /// Periods with their rate, ordered by begin
    /// </summary>
    public IReadOnlyList<MachiningRateSegment> Segments => m_segments;

    /// <summary>
    /// Rate between and after the periods, or when the rate of a period is not known
    /// </summary>
    public double? DefaultRate { get; }

    /// <summary>
    /// How long it takes, from a date/time, to machine a number of seconds
    /// </summary>
    /// <param name="from">in UTC</param>
    /// <param name="seconds">machining seconds that are left to reach</param>
    /// <returns>null when the number of seconds is never reached</returns>
    public TimeSpan? GetDuration (DateTime from, int seconds)
    {
      if (seconds <= 0) {
        return TimeSpan.Zero;
      }

      double remaining = seconds;
      var cursor = from;
      foreach (var segment in m_segments) {
        if (segment.End.HasValue && (segment.End.Value <= cursor)) {
          continue;
        }
        if (cursor < segment.Begin) {
          // Nothing is planned up to the period
          if (TryReach (cursor, segment.Begin, this.DefaultRate, ref remaining, out var reachedInGap)) {
            return reachedInGap.Subtract (from);
          }
          cursor = segment.Begin;
        }
        if (TryReach (cursor, segment.End, segment.Rate ?? this.DefaultRate, ref remaining, out var reached)) {
          return reached.Subtract (from);
        }
        if (!segment.End.HasValue) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetDuration: no machining expected after {cursor} => not reached");
          }
          return null;
        }
        cursor = segment.End.Value;
      }

      // After the last period
      if (TryReach (cursor, null, this.DefaultRate, ref remaining, out var reachedAfter)) {
        return reachedAfter.Subtract (from);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetDuration: no default rate after {cursor} => not reached");
      }
      return null;
    }

    /// <summary>
    /// Try to reach the machining seconds that are left in a period
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end">null: no end</param>
    /// <param name="rate">may be null</param>
    /// <param name="remaining">machining seconds that are left, reduced by what the period machines when it is not reached</param>
    /// <param name="reached">date/time the machining seconds are reached at</param>
    /// <returns>the machining seconds are reached in the period</returns>
    static bool TryReach (DateTime begin, DateTime? end, double? rate, ref double remaining, out DateTime reached)
    {
      reached = begin;
      if (!rate.HasValue || (rate.Value <= 0)) {
        // Nothing is machined in the period
        return false;
      }

      var neededSeconds = remaining / rate.Value;
      if (end.HasValue) {
        var periodSeconds = end.Value.Subtract (begin).TotalSeconds;
        if (periodSeconds < neededSeconds) {
          remaining -= periodSeconds * rate.Value;
          return false;
        }
      }
      if (DateTime.MaxValue.Subtract (begin).TotalSeconds <= neededSeconds) {
        // Too far away to be represented
        return false;
      }
      reached = begin.AddSeconds (neededSeconds);
      return true;
    }
  }
}
