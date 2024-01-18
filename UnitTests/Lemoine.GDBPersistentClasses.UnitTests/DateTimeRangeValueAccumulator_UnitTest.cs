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
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (1, 2)));
          Assert.That (values[i].Value, Is.EqualTo (1));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (3, 4)));
          Assert.That (values[i].Value, Is.EqualTo (1));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (4, 5)));
          Assert.That (values[i].Value, Is.EqualTo (2));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (5, 6)));
          Assert.That (values[i].Value, Is.EqualTo (1));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (6, 7)));
          Assert.That (values[i].Value, Is.EqualTo (2));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (7, 8)));
          Assert.That (values[i].Value, Is.EqualTo (1));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (9, 10)));
          Assert.That (values[i].Value, Is.EqualTo (1));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (10, 11)));
          Assert.That (values[i].Value, Is.EqualTo (2));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (values[i].Range, Is.EqualTo (R (11, 12)));
          Assert.That (values[i].Value, Is.EqualTo (1));
        });
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
