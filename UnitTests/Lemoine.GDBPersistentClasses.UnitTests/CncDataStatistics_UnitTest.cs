// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Lemoine.Model;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="CncDataStatistics"/> and <see cref="CncDataWeek"/>
  /// </summary>
  [TestFixture]
  public class CncDataStatistics_UnitTest
  {
    const double TOLERANCE = 1e-9;

    /// <summary>
    /// The combination of two consecutive series gives the statistics of the whole series
    /// </summary>
    [Test]
    public void TestCombine ()
    {
      var first = new double[] { 1.0, 4.0, 2.5, 8.0 };
      var next = new double[] { -3.0, 7.5, 6.0 };
      var all = first.Concat (next).ToArray ();

      var combined = CncDataStatistics.Combine (CncDataStatistics.FromValues (first), CncDataStatistics.FromValues (next));

      var average = all.Average ();
      var stdDev = Math.Sqrt (all.Select (v => (v - average) * (v - average)).Sum () / all.Length);
      Assert.Multiple (() => {
        Assert.That (combined.Count, Is.EqualTo (all.Length));
        Assert.That (combined.Average, Is.EqualTo (average).Within (TOLERANCE));
        Assert.That (combined.Min, Is.EqualTo (-3.0));
        Assert.That (combined.Max, Is.EqualTo (8.0));
        Assert.That (combined.StdDev, Is.EqualTo (stdDev).Within (TOLERANCE));
        Assert.That (combined.LastValue, Is.EqualTo (6.0));
      });
    }

    /// <summary>
    /// Statistics of a single value
    /// </summary>
    [Test]
    public void TestFromValue ()
    {
      var statistics = CncDataStatistics.FromValue (3.5);
      Assert.Multiple (() => {
        Assert.That (statistics.Count, Is.EqualTo (1));
        Assert.That (statistics.Average, Is.EqualTo (3.5));
        Assert.That (statistics.StdDev, Is.EqualTo (0.0));
        Assert.That (statistics.LastValue, Is.EqualTo (3.5));
      });
    }

    /// <summary>
    /// Invalid statistics
    /// </summary>
    [Test]
    public void TestInvalid ()
    {
      Assert.Multiple (() => {
        Assert.Throws<ArgumentOutOfRangeException> (() => new CncDataStatistics (0, 1.0, 1.0, 1.0, 0.0, 1.0));
        Assert.Throws<ArgumentException> (() => new CncDataStatistics (2, 1.0, 2.0, 1.0, 0.0, 1.0));
        Assert.Throws<ArgumentException> (() => CncDataStatistics.FromValues (new double[0]));
      });
    }

    /// <summary>
    /// Week start, same as the SQL function cncdataweekstart
    /// </summary>
    [Test]
    public void TestWeekStart ()
    {
      var monday = new DateTime (2026, 10, 12, 15, 30, 00, DateTimeKind.Utc);
      Assert.Multiple (() => {
        Assert.That (CncDataWeek.GetWeekStart (monday, DayOfWeek.Sunday), Is.EqualTo (new DateTime (2026, 10, 11, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That (CncDataWeek.GetWeekStart (monday, DayOfWeek.Monday), Is.EqualTo (new DateTime (2026, 10, 12, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That (CncDataWeek.GetWeekStart (monday, DayOfWeek.Tuesday), Is.EqualTo (new DateTime (2026, 10, 06, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That (CncDataWeek.GetWeekEnd (monday, DayOfWeek.Sunday), Is.EqualTo (new DateTime (2026, 10, 18, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That (CncDataWeek.GetWeekStart (new DateTime (2026, 10, 11, 0, 0, 0, DateTimeKind.Utc), DayOfWeek.Sunday),
          Is.EqualTo (new DateTime (2026, 10, 11, 0, 0, 0, DateTimeKind.Utc)));
      });
    }

    /// <summary>
    /// Truncation to the millisecond
    /// </summary>
    [Test]
    public void TestTruncateToMilliseconds ()
    {
      var dateTime = new DateTime (2026, 10, 12, 15, 30, 00, 123, DateTimeKind.Utc).AddTicks (4567);
      var truncated = CncDataWeek.TruncateToMilliseconds (dateTime);
      Assert.Multiple (() => {
        Assert.That (truncated, Is.EqualTo (new DateTime (2026, 10, 12, 15, 30, 00, 123, DateTimeKind.Utc)));
        Assert.That (truncated.Kind, Is.EqualTo (DateTimeKind.Utc));
      });
    }
  }
}
