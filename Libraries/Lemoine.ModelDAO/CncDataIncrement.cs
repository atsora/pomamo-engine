// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// New numeric data of a field imported in table cncdataperiod, independently of the periods they were stored in:
  /// a value that was in effect over [Begin, End] and the values that were received at End.
  ///
  /// The increments are only added: a period that is extended, split or merged does not change the increments
  /// that were already given, which allows an incremental aggregation (for example by time slot) without reading the periods again.
  /// <item>Change aggregation: the previous value lasts up to the new value: [previous date/time, End] with Value = previous value,
  ///       then [End, End] with the new value in Received</item>
  /// <item>Statistics aggregation: [end of the previous data, End] with the statistics of the new values (Value = their average)</item>
  ///
  /// Contiguous increments are combined while their values remain within the LimitDeviation of the field
  /// (Value: time-weighted average, Received: combined statistics), with the same rule as the periods:
  /// a difference larger than LimitDeviation, or no LimitDeviation, starts a new increment
  /// </summary>
  public sealed class CncDataIncrement
  {
    /// <summary>
    /// UTC begin (included)
    /// </summary>
    public DateTime Begin { get; }

    /// <summary>
    /// UTC end, after or equal to Begin. The received values are at this date/time
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Numeric value in effect over [Begin, End], to weight by the duration (time-weighted average).
    /// It may be null for a duration of zero
    /// </summary>
    public double? Value { get; }

    /// <summary>
    /// Statistics of the values that were received at End (count, minimum, maximum...), null if none
    /// </summary>
    public CncDataStatistics? Received { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="begin">UTC</param>
    /// <param name="end">UTC, after or equal to begin</param>
    /// <param name="value"></param>
    /// <param name="received"></param>
    public CncDataIncrement (DateTime begin, DateTime end, double? value, CncDataStatistics? received)
    {
      if (end < begin) {
        throw new ArgumentException ($"End {end:o} before begin {begin:o}", nameof (end));
      }
      this.Begin = begin;
      this.End = end;
      this.Value = value;
      this.Received = received;
    }

    /// <summary>
    /// Duration of the range
    /// </summary>
    public TimeSpan Duration => this.End.Subtract (this.Begin);

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    public override string ToString () => $"[CncDataIncrement {this.Begin:o}-{this.End:o} value={this.Value} received={this.Received}]";
  }
}
