// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class SlotWithDayShift.
  /// </summary>
  [TestFixture]
  public class SlotWithDayShift_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SlotWithDayShift_UnitTest).FullName);

    /// <summary>
    /// Test the Combine method
    /// </summary>
    [Test]
    public void TestCombine()
    {
      IShift shiftA = new Shift ();
      shiftA.Name = "A";
      IShift shiftB = new Shift ();
      shiftB.Name = "B";
      IShift shiftC = new Shift ();
      shiftC.Name = "C";
      IShift shiftD = new Shift ();
      shiftD.Name = "D";

      IList<ISlotWithDayShift> days = new List<ISlotWithDayShift> ();
      IList<ISlotWithDayShift> shifts = new List<ISlotWithDayShift> ();

      {
        ISlotWithDayShift day = new SlotWithDayShift (R(0, 12));
        day.Day = D(1);
        days.Add (day);
      }
      {
        ISlotWithDayShift day = new SlotWithDayShift (R(12, 20));
        day.Day = D(2);
        days.Add (day);
      }
      
      {
        ISlotWithDayShift shift = new SlotWithDayShift (R(0, 1));
        shift.Shift = shiftA;
        shifts.Add (shift);
      }
      {
        ISlotWithDayShift shift = new SlotWithDayShift (R(1, 2));
        shift.Shift = shiftB;
        shifts.Add (shift);
      }
      {
        ISlotWithDayShift shift = new SlotWithDayShift (R(2, 15));
        shift.Shift = shiftB;
        shifts.Add (shift);
      }
      {
        ISlotWithDayShift shift = new SlotWithDayShift (R(15, 16));
        shift.Shift = shiftC;
        shifts.Add (shift);
      }
      {
        ISlotWithDayShift shift = new SlotWithDayShift (R(16, 20));
        shift.Shift = shiftC;
        shifts.Add (shift);
      }
      
      {
        IList<ISlotWithDayShift> combined = SlotWithDayShift.Combine (shifts, days);
        Assert.AreEqual (4, combined.Count);
        int i = 0;
        Assert.AreEqual (D(1), combined [i].Day);
        Assert.AreEqual (shiftA, combined [i].Shift);
        Assert.AreEqual (R(0, 1), combined [i].DateTimeRange);
        ++i;
        Assert.AreEqual (D(1), combined [i].Day);
        Assert.AreEqual (shiftB, combined [i].Shift);
        Assert.AreEqual (R(1, 12), combined [i].DateTimeRange);
        ++i;
        Assert.AreEqual (D(2), combined [i].Day);
        Assert.AreEqual (shiftB, combined [i].Shift);
        Assert.AreEqual (R(12, 15), combined [i].DateTimeRange);
        ++i;
        Assert.AreEqual (D(2), combined [i].Day);
        Assert.AreEqual (shiftC, combined [i].Shift);
        Assert.AreEqual (R(15, 20), combined [i].DateTimeRange);
        ++i;
      }
    }
    
    DateTime T(int i)
    {
      return new DateTime (2015, 01, 01, 00, i, 00, DateTimeKind.Utc);
    }
    
    UtcDateTimeRange R(int l, int u)
    {
      return new UtcDateTimeRange (T(l), T(u));
    }
    
    DateTime D(int i)
    {
      return new DateTime (2015, 01, i);
    }
  }
}
