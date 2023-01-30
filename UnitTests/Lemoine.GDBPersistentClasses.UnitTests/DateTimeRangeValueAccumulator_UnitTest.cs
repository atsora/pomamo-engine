// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.Core.Log;
using NUnit.Framework;
using Pulse.Extensions.Database.Accumulator.Impl;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DateTimeRangeValueAccumulator.
  /// </summary>
  [TestFixture]
  public class DateTimeRangeValueAccumulator_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DateTimeRangeValueAccumulator_UnitTest).FullName);

    class TestAccumulator: DateTimeRangeValueAccumulator<int, int>
    {
      public TestAccumulator ()
        : base (a => a,
                (a, b) => a+b,
                a => -a,
                (a, b) => a-b,
                a => (0 == a))
      {
      }
      
      public override void Store (string transactionName)
      {
        return;
      }
    }
    
    /// <summary>
    /// Test Add/Remove
    /// 
    /// 1 2 3 4 5 6 7 8 9 10 11 12
    ///       ++++++++  +++++++++
    /// ++  ++++  +++++++++++
    ///             ------
    /// </summary>
    [Test]
    public void TestAddRemove ()
    {
      var accumulator = new TestAccumulator ();
      accumulator.Add (R(4, 8), 1);
      accumulator.Add (R(9, 12), 1);
      accumulator.Add (R(1, 2), 1);
      accumulator.Add (R(3, 5), 1);
      accumulator.Add (R(6, 11), 1);
      accumulator.Remove (R(7, 10), 1);
      accumulator.Purge ();
      
      {
        var values = accumulator.DateTimeRangeValues;
        int i = 0;
        Assert.AreEqual (R(1, 2), values[i].Range);
        Assert.AreEqual (1, values[i].Value);
        ++i;
        Assert.AreEqual (R(3, 4), values[i].Range);
        Assert.AreEqual (1, values[i].Value);
        ++i;
        Assert.AreEqual (R(4, 5), values[i].Range);
        Assert.AreEqual (2, values[i].Value);
        ++i;
        Assert.AreEqual (R(5, 6), values[i].Range);
        Assert.AreEqual (1, values[i].Value);
        ++i;
        Assert.AreEqual (R(6, 7), values[i].Range);
        Assert.AreEqual (2, values[i].Value);
        ++i;
        Assert.AreEqual (R(7, 8), values[i].Range);
        Assert.AreEqual (1, values[i].Value);
        ++i;
        Assert.AreEqual (R(9, 10), values[i].Range);
        Assert.AreEqual (1, values[i].Value);
        ++i;
        Assert.AreEqual (R(10, 11), values[i].Range);
        Assert.AreEqual (2, values[i].Value);
        ++i;
        Assert.AreEqual (R(11, 12), values[i].Range);
        Assert.AreEqual (1, values[i].Value);
      }
    }
    
    DateTime T(int i)
    {
      return new DateTime (2014, 12, 31, 00, 00, 00, DateTimeKind.Utc).AddDays (i);
    }
    
    UtcDateTimeRange R(int l, int u)
    {
      return new UtcDateTimeRange (T (l), T (u));
    }
  }
}
