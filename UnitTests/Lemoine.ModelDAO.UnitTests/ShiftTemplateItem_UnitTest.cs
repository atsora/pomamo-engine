// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;

using Lemoine.Core.Log;
using Lemoine.Model;
using NUnit.Framework;

namespace Lemoine.ModelDAO.UnitTests
{
  /// <summary>
  /// Unit tests for the applicability criteria and the ordering of <see cref="IShiftTemplateItem"/>
  /// </summary>
  [TestFixture]
  public class ShiftTemplateItem_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftTemplateItem_UnitTest).FullName);

    /// <summary>
    /// Minimal implementation of <see cref="IShiftTemplateItem"/> to test the criteria
    /// without any database
    /// </summary>
    class Item : IShiftTemplateItem
    {
      public int Id { get; set; }
      public int Version => 0;
      public IShift Shift { get; set; }
      public IShiftTemplate SubShiftTemplate { get; set; }
      public WeekDay WeekDays { get; set; } = WeekDay.AllDays;
      public TimePeriodOfDay TimePeriod { get; set; }
      public DateTime? Day { get; set; }
      public int? WeekYear { get; set; }
      public int? WeekNumber { get; set; }
      public int? WeekFrequency { get; set; }

      public void Unproxy () { }
    }

    static DateTime D (int year, int month, int day) => new DateTime (year, month, day, 0, 0, 0, DateTimeKind.Local);

    /// <summary>
    /// An item without any criteria applies any day
    /// </summary>
    [Test]
    public void TestNoRestriction ()
    {
      var item = new Item ();

      Assert.Multiple (() => {
        Assert.That (item.GetPriority (), Is.EqualTo (0));
        Assert.That (item.IsDayApplicable (D (2026, 08, 18), 2026, 34), Is.True);
      });
    }

    /// <summary>
    /// Test an item that is restricted to one specific week
    /// </summary>
    [Test]
    public void TestSpecificWeek ()
    {
      var item = new Item { WeekYear = 2026, WeekNumber = 34 };

      Assert.Multiple (() => {
        Assert.That (item.GetPriority (), Is.EqualTo (1));
        Assert.That (item.IsDayApplicable (D (2026, 08, 18), 2026, 34), Is.True);
        Assert.That (item.IsDayApplicable (D (2026, 08, 25), 2026, 35), Is.False);
        Assert.That (item.IsDayApplicable (D (2026, 08, 11), 2026, 33), Is.False, "Before the reference week");
        Assert.That (item.IsDayApplicable (D (2027, 08, 24), 2027, 34), Is.False, "Same week number, next year");
      });
    }

    /// <summary>
    /// Test an item that is repeated every two weeks from a specific week
    /// </summary>
    [Test]
    public void TestWeekFrequency ()
    {
      var item = new Item { WeekYear = 2026, WeekNumber = 34, WeekFrequency = 2 };

      Assert.Multiple (() => {
        Assert.That (item.IsDayApplicable (D (2026, 08, 18), 2026, 34), Is.True);
        Assert.That (item.IsDayApplicable (D (2026, 08, 25), 2026, 35), Is.False);
        Assert.That (item.IsDayApplicable (D (2026, 09, 01), 2026, 36), Is.True);
        Assert.That (item.IsDayApplicable (D (2026, 08, 11), 2026, 33), Is.False);
        // The week 1 of 2027 is 20 weeks after the week 34 of 2026
        Assert.That (item.IsDayApplicable (D (2027, 01, 05), 2027, 1), Is.True);
      });
    }

    /// <summary>
    /// An item with a week number but no week year applies to that week number every year
    /// </summary>
    [Test]
    public void TestWeekNumberWithoutYear ()
    {
      var item = new Item { WeekNumber = 34 };

      Assert.Multiple (() => {
        Assert.That (item.IsDayApplicable (D (2026, 08, 18), 2026, 34), Is.True);
        Assert.That (item.IsDayApplicable (D (2027, 08, 24), 2027, 34), Is.True);
        Assert.That (item.IsDayApplicable (D (2026, 08, 25), 2026, 35), Is.False);
      });
    }

    /// <summary>
    /// The week days are combined with the week criteria
    /// </summary>
    [Test]
    public void TestWeekDaysAndSpecificWeek ()
    {
      var item = new Item {
        WeekDays = DayOfWeek.Monday.ConvertToWeekDay (),
        WeekYear = 2026,
        WeekNumber = 34
      };

      Assert.Multiple (() => {
        Assert.That (item.IsDayApplicable (D (2026, 08, 17), 2026, 34), Is.True, "Monday of the week 34");
        Assert.That (item.IsDayApplicable (D (2026, 08, 18), 2026, 34), Is.False, "Tuesday of the week 34");
        Assert.That (item.IsDayApplicable (D (2026, 08, 24), 2026, 35), Is.False, "Monday of the week 35");
      });
    }

    /// <summary>
    /// The week days are not considered when a specific day is set
    /// </summary>
    [Test]
    public void TestSpecificDay ()
    {
      var item = new Item { Day = D (2026, 12, 25) };

      Assert.Multiple (() => {
        Assert.That (item.GetPriority (), Is.EqualTo (2));
        Assert.That (item.IsDayApplicable (D (2026, 12, 25), 2026, 52), Is.True);
        Assert.That (item.IsDayApplicable (D (2027, 12, 25), 2027, 51), Is.False);
      });
    }

    /// <summary>
    /// The items are applied in this order, the last ones overriding the previous ones:
    /// no criteria, then specific weeks by increasing reference week, then specific days
    /// </summary>
    [Test]
    public void TestOrdering ()
    {
      var noCriteria = new Item { Id = 1 };
      var weekDays = new Item { Id = 2, WeekDays = DayOfWeek.Monday.ConvertToWeekDay () };
      var week2027 = new Item { Id = 3, WeekYear = 2027, WeekNumber = 2 };
      var week2026 = new Item { Id = 4, WeekYear = 2026, WeekNumber = 34 };
      var anyYearWeek = new Item { Id = 5, WeekNumber = 10 };
      var day = new Item { Id = 6, Day = D (2026, 12, 25) };

      var items = new IShiftTemplateItem[] { day, week2027, anyYearWeek, weekDays, week2026, noCriteria };

      var sorted = items
        .OrderBy (i => i.GetPriority ())
        .ThenBy (i => i.GetWeekSortKey ())
        .Select (i => i.Id)
        .ToList ();

      // 1 and 2 have no week criteria (their relative order is the initial one),
      // then 5 (any year), then 4 (2026-34), then 3 (2027-02), then 6 (specific day)
      Assert.That (sorted, Is.EqualTo (new[] { 2, 1, 5, 4, 3, 6 }));
    }
  }
}
