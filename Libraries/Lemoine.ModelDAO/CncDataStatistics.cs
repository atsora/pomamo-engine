// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Combinable statistics of a series of numerical values
  ///
  /// Two consecutive series are combined with the parallel algorithm of Chan
  /// (no need to keep the values)
  /// </summary>
  public readonly struct CncDataStatistics
  {
    /// <summary>
    /// Number of values (strictly positive)
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Average
    /// </summary>
    public double Average { get; }

    /// <summary>
    /// Minimum value
    /// </summary>
    public double Min { get; }

    /// <summary>
    /// Maximum value
    /// </summary>
    public double Max { get; }

    /// <summary>
    /// Population standard deviation
    /// </summary>
    public double StdDev { get; }

    /// <summary>
    /// Last value of the series
    /// </summary>
    public double LastValue { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="count">strictly positive</param>
    /// <param name="average"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="stdDev">population standard deviation, positive</param>
    /// <param name="lastValue"></param>
    public CncDataStatistics (int count, double average, double min, double max, double stdDev, double lastValue)
    {
      if (count <= 0) {
        throw new ArgumentOutOfRangeException (nameof (count), count, "The number of values must be strictly positive");
      }
      if (max < min) {
        throw new ArgumentException ($"Max {max} is less than Min {min}", nameof (max));
      }
      if (stdDev < 0) {
        throw new ArgumentOutOfRangeException (nameof (stdDev), stdDev, "The standard deviation can't be negative");
      }
      this.Count = count;
      this.Average = average;
      this.Min = min;
      this.Max = max;
      this.StdDev = stdDev;
      this.LastValue = lastValue;
    }

    /// <summary>
    /// Statistics of a single value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static CncDataStatistics FromValue (double value) => new CncDataStatistics (1, value, value, value, 0.0, value);

    /// <summary>
    /// Statistics of a series of values, in their order
    /// </summary>
    /// <param name="values">not empty</param>
    /// <returns></returns>
    public static CncDataStatistics FromValues (IEnumerable<double> values)
    {
      CncDataStatistics? result = null;
      foreach (var value in values) {
        var single = FromValue (value);
        result = result.HasValue ? Combine (result.Value, single) : single;
      }
      return result ?? throw new ArgumentException ("No value", nameof (values));
    }

    /// <summary>
    /// Combine two consecutive series: a before b
    /// </summary>
    /// <param name="a">first series</param>
    /// <param name="b">next series</param>
    /// <returns></returns>
    public static CncDataStatistics Combine (CncDataStatistics a, CncDataStatistics b)
    {
      var count = a.Count + b.Count;
      var delta = b.Average - a.Average;
      var average = a.Average + delta * b.Count / count;
      // Sum of the squared differences to the average
      var m2 = a.StdDev * a.StdDev * a.Count
        + b.StdDev * b.StdDev * b.Count
        + delta * delta * a.Count * b.Count / count;
      var stdDev = Math.Sqrt (Math.Max (0.0, m2 / count));
      return new CncDataStatistics (count, average, Math.Min (a.Min, b.Min), Math.Max (a.Max, b.Max), stdDev, b.LastValue);
    }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[n={this.Count} avg={this.Average} min={this.Min} max={this.Max} stddev={this.StdDev} last={this.LastValue}]";
  }
}
