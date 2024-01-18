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
        Assert.That (combined, Has.Count.EqualTo (4));
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (combined[i].Day, Is.EqualTo (D (1)));
          Assert.That (combined[i].Shift, Is.EqualTo (shiftA));
          Assert.That (combined[i].DateTimeRange, Is.EqualTo (R (0, 1)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (combined[i].Day, Is.EqualTo (D (1)));
          Assert.That (combined[i].Shift, Is.EqualTo (shiftB));
          Assert.That (combined[i].DateTimeRange, Is.EqualTo (R (1, 12)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (combined[i].Day, Is.EqualTo (D (2)));
          Assert.That (combined[i].Shift, Is.EqualTo (shiftB));
          Assert.That (combined[i].DateTimeRange, Is.EqualTo (R (12, 15)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (combined[i].Day, Is.EqualTo (D (2)));
          Assert.That (combined[i].Shift, Is.EqualTo (shiftC));
          Assert.That (combined[i].DateTimeRange, Is.EqualTo (R (15, 20)));
        });
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
