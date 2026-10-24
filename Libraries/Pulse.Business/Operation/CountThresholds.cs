// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// A full cycle that is counted, see <see cref="CountThresholds"/>
  /// </summary>
  [Serializable]
  public readonly struct CountedCycle
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="end">end of the cycle, in UTC</param>
    /// <param name="quantity">parts the cycle made when it is adjusted, else null</param>
    public CountedCycle (DateTime end, int? quantity = null)
    {
      this.End = end;
      this.Quantity = quantity;
    }

    /// <summary>
    /// End of the cycle, in UTC: a part, or a cycle, counts when its cycle ends
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Parts the cycle made when it is adjusted, else null: the parts per cycle of the
    /// operation apply
    /// </summary>
    public int? Quantity { get; }
  }

  /// <summary>
  /// What a walk over the cycles counted, see <see cref="CountThresholds.Walk"/>
  /// </summary>
  [Serializable]
  public sealed class CountThresholdsResult
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="count">parts or cycles that were counted</param>
    /// <param name="dueReachedAt">when the due threshold was reached, null if it was not</param>
    /// <param name="lateReachedAt">when the late threshold was reached, null if it was not, or if there is none</param>
    public CountThresholdsResult (int count, DateTime? dueReachedAt, DateTime? lateReachedAt)
    {
      this.Count = count;
      this.DueReachedAt = dueReachedAt;
      this.LateReachedAt = lateReachedAt;
    }

    /// <summary>
    /// Parts or cycles that were counted
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// When the due threshold was reached: the end of the cycle that reached it. Null if it
    /// was not reached
    /// </summary>
    public DateTime? DueReachedAt { get; }

    /// <summary>
    /// When the late threshold was reached: the end of the cycle that reached it. Null if it
    /// was not reached, or if there is no late threshold
    /// </summary>
    public DateTime? LateReachedAt { get; }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[CountThresholdsResult {this.Count} due={this.DueReachedAt} late={this.LateReachedAt}]";
  }

  /// <summary>
  /// Count the parts, or the cycles, of a sequence of full cycles, and tell when the
  /// thresholds were reached: for example when a maintenance becomes due after a number of
  /// parts, and late after another one
  ///
  /// It is a pure function, with no database access: the caller reads the cycles
  /// </summary>
  public static class CountThresholds
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CountThresholds).FullName);

    /// <summary>
    /// Count the parts, or the cycles, of full cycles, and get the date/time each threshold
    /// was reached at
    ///
    /// A cycle counts 1 when the cycles are counted. When the parts are counted, it counts
    /// its quantity if it is adjusted, else the parts per cycle of the operation. A threshold
    /// is reached at the end of the cycle that makes the count reach it: several thresholds
    /// may be reached by the same cycle
    /// </summary>
    /// <param name="cycles">full cycles, in any order, not null</param>
    /// <param name="countsParts">are the parts counted? Else the cycles are</param>
    /// <param name="partsPerCycle">parts one cycle of the operation makes, when it is not adjusted</param>
    /// <param name="due">strictly positive count of the due threshold</param>
    /// <param name="late">count of the late threshold, not lower than due, null if there is none</param>
    /// <returns>not null</returns>
    /// <exception cref="ArgumentNullException">cycles is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">due is not strictly positive, or late is lower than due</exception>
    public static CountThresholdsResult Walk (IEnumerable<CountedCycle> cycles, bool countsParts, int partsPerCycle, int due, int? late)
    {
      if (cycles is null) {
        log.Fatal ("Walk: cycles is null");
        throw new ArgumentNullException (nameof (cycles));
      }
      if (due <= 0) {
        log.Fatal ($"Walk: due {due} is not strictly positive");
        throw new ArgumentOutOfRangeException (nameof (due), due, "due is not strictly positive");
      }
      if (late.HasValue && (late.Value < due)) {
        log.Fatal ($"Walk: late {late.Value} is lower than due {due}");
        throw new ArgumentOutOfRangeException (nameof (late), late, "late is lower than due");
      }

      var count = 0;
      DateTime? dueReachedAt = null;
      DateTime? lateReachedAt = null;
      foreach (var cycle in cycles.OrderBy (x => x.End)) {
        count += countsParts
          ? Math.Max (0, cycle.Quantity ?? partsPerCycle)
          : 1;
        if (!dueReachedAt.HasValue && (due <= count)) {
          dueReachedAt = cycle.End;
        }
        if (late.HasValue && !lateReachedAt.HasValue && (late.Value <= count)) {
          lateReachedAt = cycle.End;
        }
      }

      var result = new CountThresholdsResult (count, dueReachedAt, lateReachedAt);
      if (log.IsDebugEnabled) {
        log.Debug ($"Walk: {result}");
      }
      return result;
    }
  }
}
