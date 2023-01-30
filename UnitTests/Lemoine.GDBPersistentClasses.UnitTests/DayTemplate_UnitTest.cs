// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DayTemplate.
  /// </summary>
  [TestFixture]
  public class DayTemplate_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplate_UnitTest).FullName);

    /// <summary>
    /// Test AdjustToDayBegin
    /// </summary>
    [Test]
    public void TestAdjustToDayBegin()
    {
      DayTemplate dayTemplate = new DayTemplate ("Test");
      dayTemplate.AddItem (TimeSpan.FromHours (06), Lemoine.Model.WeekDay.Monday);
      dayTemplate.AddItem (TimeSpan.FromHours (-2), Lemoine.Model.WeekDay.Tuesday);
      dayTemplate.AddItem (TimeSpan.FromHours (03), Lemoine.Model.WeekDay.Wednesday);
      
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (1, 02));
        Assert.AreEqual (T (1, 06), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (1, 06));
        Assert.AreEqual (T (1, 06), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (1, 08));
        Assert.AreEqual (T (1, 22), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (1, 22));
        Assert.AreEqual (T (1, 22), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (1, 23));
        Assert.AreEqual (T (3, 03), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (2, 02));
        Assert.AreEqual (T (3, 03), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (2, 03));
        Assert.AreEqual (T (3, 03), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (2, 23));
        Assert.AreEqual (T (3, 03), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (3, 02));
        Assert.AreEqual (T (3, 03), dayBegin);
      }
      { // Test
        DateTime dayBegin = dayTemplate.AdjustToDayBegin (T (3, 03));
        Assert.AreEqual (T (3, 03), dayBegin);
      }
    }
    
    /// <summary>
    /// Get a UTC date/time for the tests
    /// </summary>
    /// <param name="day">1: Monday</param>
    /// <param name="localHour"></param>
    /// <returns></returns>
    DateTime T(int day, int localHour)
    {
      return new DateTime (2015, 11, day+1, localHour, 00, 00, DateTimeKind.Local).ToUniversalTime ();
    }
  }
}
