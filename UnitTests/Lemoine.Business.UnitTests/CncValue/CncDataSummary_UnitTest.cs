// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Business.CncValue;
using Lemoine.Model;
using NUnit.Framework;

namespace Lemoine.Business.UnitTests.CncValue
{
  /// <summary>
  /// Unit tests for the computation of <see cref="CncDataSummary"/> (without any database)
  /// </summary>
  [TestFixture]
  public class CncDataSummary_UnitTest
  {
    static readonly DateTime T0 = new DateTime (2026, 10, 15, 10, 00, 00, DateTimeKind.Utc);

    sealed class Period : ICncDataPeriodValue
    {
      public long Id { get; set; }
      public DateTime Begin { get; set; }
      public DateTime End { get; set; }
      public bool Stopped { get; set; }
      public int? Count { get; set; }
      public double? Average { get; set; }
      public double? Min { get; set; }
      public double? Max { get; set; }
      public double? StdDev { get; set; }
      public double? LastValue { get; set; }
      public string StringValue { get; set; }
      public string JsonValue { get; set; }
      public bool Archived { get; set; }
      public UtcDateTimeRange DateTimeRange => new UtcDateTimeRange (this.Begin, this.End);
    }

    static Period P (int begin, int end, double average, double min, double max, int count) => new Period {
      Begin = T0.AddSeconds (begin),
      End = T0.AddSeconds (end),
      Average = average,
      Min = min,
      Max = max,
      Count = count,
      LastValue = average,
    };

    /// <summary>
    /// Periods only: average weighted by the overlap duration, work, prorated count, minimum and maximum,
    /// time above the high threshold, effective threshold
    /// </summary>
    [Test]
    public void TestPeriods ()
    {
      var range = new UtcDateTimeRange (T0, T0.AddSeconds (60));
      var periods = new List<ICncDataPeriodValue> {
        P (-30, 30, 10.0, 5.0, 15.0, 60), // Half in the range
        P (30, 60, 40.0, 35.0, 45.0, 30),
      };
      var summary = CncDataSummary.Compute (null, range, periods, new List<CncDataSampleValue> (), null, 20.0);
      Assert.Multiple (() => {
        Assert.That (summary.Duration, Is.EqualTo (TimeSpan.FromSeconds (60)));
        Assert.That (summary.Average, Is.EqualTo (25.0).Within (1e-9), "10 during 30 s and 40 during 30 s");
        Assert.That (summary.Work, Is.EqualTo (1500.0).Within (1e-9));
        Assert.That (summary.Count, Is.EqualTo (60.0).Within (1e-9), "half of 60 + 30");
        Assert.That (summary.Min, Is.EqualTo (5.0));
        Assert.That (summary.Max, Is.EqualTo (45.0));
        Assert.That (summary.TimeAboveHigh, Is.EqualTo (TimeSpan.FromSeconds (30)));
        Assert.That (summary.Median, Is.Null, "no sample");
        Assert.That (summary.P95, Is.Null);
      });

      // Effective threshold: the first period (average 10) is not considered
      var effective = CncDataSummary.Compute (null, range, periods, new List<CncDataSampleValue> (), 20.0, null);
      Assert.Multiple (() => {
        Assert.That (effective.Duration, Is.EqualTo (TimeSpan.FromSeconds (30)));
        Assert.That (effective.Average, Is.EqualTo (40.0).Within (1e-9));
        Assert.That (effective.TimeAboveHigh, Is.Null, "no high threshold");
      });
    }

    /// <summary>
    /// Samples: median, 95th percentile, minimum and maximum, time above the high threshold, effective threshold
    /// </summary>
    [Test]
    public void TestSamples ()
    {
      var range = new UtcDateTimeRange (T0, T0.AddSeconds (1));
      // 100 samples every 10 ms: 1, 2, ..., 100
      var samples = Enumerable.Range (1, 100)
        .Select (i => new CncDataSampleValue (T0.AddMilliseconds (10 * (i - 1)), (double)i))
        .ToList ();
      samples.Add (new CncDataSampleValue (T0.AddSeconds (2), 1000.0)); // Outside the range
      var summary = CncDataSummary.Compute (null, range, new List<ICncDataPeriodValue> (), samples, null, 90.0);
      Assert.Multiple (() => {
        Assert.That (summary.SampleCount, Is.EqualTo (100));
        Assert.That (summary.Median, Is.EqualTo (50.5).Within (1e-9));
        Assert.That (summary.P95, Is.EqualTo (95.05).Within (1e-9));
        Assert.That (summary.Min, Is.EqualTo (1.0));
        Assert.That (summary.Max, Is.EqualTo (100.0));
        Assert.That (summary.TimeAboveHigh.Value.TotalMilliseconds, Is.EqualTo (100.0).Within (1e-6), "10 samples above 90, every 10 ms");
      });

      var effective = CncDataSummary.Compute (null, range, new List<ICncDataPeriodValue> (), samples, 51.0, null);
      Assert.Multiple (() => {
        Assert.That (effective.SampleCount, Is.EqualTo (50));
        Assert.That (effective.Min, Is.EqualTo (51.0));
        Assert.That (effective.Median, Is.EqualTo (75.5).Within (1e-9));
      });
    }
  }
}
