// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Lemoine.Model;
using NUnit.Framework;

namespace Lemoine.ModelDAO.UnitTests
{
  /// <summary>
  /// Unit tests for the applicability criteria of <see cref="IMachineStateTemplateItem"/>
  /// </summary>
  [TestFixture]
  public class MachineStateTemplateItem_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateItem_UnitTest).FullName);

    /// <summary>
    /// Minimal implementation of <see cref="IMachineStateTemplateItem"/> to test the criteria
    /// without any database
    /// </summary>
    class Item : IMachineStateTemplateItem
    {
      public int Id => 0;
      public int Version => 0;
      public int Order => 0;
      public IMachineObservationState MachineObservationState { get; set; }
      public IMachineStateTemplate SubMachineStateTemplate { get; set; }
      public IShift Shift { get; set; }
      public WeekDay WeekDays { get; set; } = WeekDay.AllDays;
      public TimePeriodOfDay TimePeriod { get; set; }
      public DateTime? Day { get; set; }
      public int? WeekYear { get; set; }
      public int? WeekNumber { get; set; }
      public int? WeekFrequency { get; set; }
      public bool YearlyRepeat { get; set; }

      public void Unproxy () { }
    }

    static DateTime D (int year, int month, int day) => new DateTime (year, month, day, 0, 0, 0, DateTimeKind.Local);

    /// <summary>
    /// Test the week numbers that are computed with the default (ISO 8601) configuration
    /// </summary>
    [Test]
    public void TestWeekNumber ()
    {
      Assert.Multiple (() => {
        // 2026-01-01 is a Thursday: it is in the week 1 of 2026
        WeekNumberHelper.GetWeek (D (2026, 01, 01), out var weekYear, out var weekNumber);
        Assert.That (weekYear, Is.EqualTo (2026));
        Assert.That (weekNumber, Is.EqualTo (1));
        // 2026-08-18 is a Tuesday of the week 34
        Assert.That (WeekNumberHelper.GetWeekNumber (D (2026, 08, 18)), Is.EqualTo (34));
        // The week starts on Monday by default
        Assert.That (WeekNumberHelper.GetWeekStart (D (2026, 08, 18)), Is.EqualTo (D (2026, 08, 17)));
        Assert.That (WeekNumberHelper.GetWeekStart (2026, 34).Date, Is.EqualTo (D (2026, 08, 17).Date));
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
        Assert.That (item.IsDayApplicable (D (2026, 08, 17)), Is.True, "Monday of the week 34");
        Assert.That (item.IsDayApplicable (D (2026, 08, 23)), Is.True, "Sunday of the week 34");
        Assert.That (item.IsDayApplicable (D (2026, 08, 16)), Is.False, "Sunday of the week 33");
        Assert.That (item.IsDayApplicable (D (2026, 08, 24)), Is.False, "Monday of the week 35");
        Assert.That (item.IsDayApplicable (D (2027, 08, 23)), Is.False, "Week 34 of the next year");
        Assert.That (item.HasDayRestriction (), Is.True);
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
        Assert.That (item.IsDayApplicable (D (2026, 08, 18)), Is.True, "Week 34");
        Assert.That (item.IsDayApplicable (D (2026, 08, 25)), Is.False, "Week 35");
        Assert.That (item.IsDayApplicable (D (2026, 09, 01)), Is.True, "Week 36");
        Assert.That (item.IsDayApplicable (D (2026, 09, 08)), Is.False, "Week 37");
        Assert.That (item.IsDayApplicable (D (2026, 08, 11)), Is.False, "Week 33, before the reference week");
        // 2027-01-05 is in the week 1 of 2027, which is 20 weeks after the week 34 of 2026
        Assert.That (item.IsDayApplicable (D (2027, 01, 05)), Is.True, "Week 1 of 2027");
      });
    }

    /// <summary>
    /// Test an item that is applicable every week from a specific week
    /// </summary>
    [Test]
    public void TestEveryWeek ()
    {
      var item = new Item { WeekYear = 2026, WeekNumber = 34, WeekFrequency = 1 };

      Assert.Multiple (() => {
        Assert.That (item.IsDayApplicable (D (2026, 08, 18)), Is.True);
        Assert.That (item.IsDayApplicable (D (2026, 08, 25)), Is.True);
        Assert.That (item.IsDayApplicable (D (2026, 08, 11)), Is.False, "Before the reference week");
      });
    }

    /// <summary>
    /// Test an item that is restricted to a week number, whatever the year is
    /// </summary>
    [Test]
    public void TestYearlyWeek ()
    {
      var item = new Item { WeekYear = 2026, WeekNumber = 34, YearlyRepeat = true };

      Assert.Multiple (() => {
        Assert.That (item.IsDayApplicable (D (2026, 08, 18)), Is.True);
        Assert.That (item.IsDayApplicable (D (2027, 08, 24)), Is.True, "Week 34 of 2027");
        Assert.That (item.IsDayApplicable (D (2025, 08, 19)), Is.True, "Week 34 of 2025");
        Assert.That (item.IsDayApplicable (D (2026, 08, 25)), Is.False, "Week 35");
      });
    }

    /// <summary>
    /// Test an item that is restricted to a specific day, repeated every year (public holiday)
    /// </summary>
    [Test]
    public void TestYearlyDay ()
    {
      var item = new Item { Day = D (2026, 12, 25), YearlyRepeat = true };

      Assert.Multiple (() => {
        Assert.That (item.IsDayApplicable (D (2026, 12, 25)), Is.True);
        Assert.That (item.IsDayApplicable (D (2030, 12, 25)), Is.True);
        Assert.That (item.IsDayApplicable (D (2030, 12, 24)), Is.False);
        Assert.That (item.GetYearlyDay (2030), Is.EqualTo (D (2030, 12, 25)));
      });
    }

    /// <summary>
    /// Test an item that is restricted to a unique specific day
    /// </summary>
    [Test]
    public void TestSpecificDay ()
    {
      var item = new Item { Day = D (2026, 12, 25) };

      Assert.Multiple (() => {
        Assert.That (item.IsDayApplicable (D (2026, 12, 25)), Is.True);
        Assert.That (item.IsDayApplicable (D (2027, 12, 25)), Is.False);
      });
    }

    /// <summary>
    /// February 29th does not exist every year
    /// </summary>
    [Test]
    public void TestYearlyDayOnFebruary29 ()
    {
      var item = new Item { Day = D (2024, 02, 29), YearlyRepeat = true };

      Assert.Multiple (() => {
        Assert.That (item.GetYearlyDay (2028), Is.EqualTo (D (2028, 02, 29)));
        Assert.That (item.GetYearlyDay (2026), Is.Null);
        Assert.That (item.IsDayApplicable (D (2026, 02, 28)), Is.False);
      });
    }

    /// <summary>
    /// Test the combination of the week days and of a specific week
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
        Assert.That (item.IsDayApplicable (D (2026, 08, 17)), Is.True, "Monday of the week 34");
        Assert.That (item.IsDayApplicable (D (2026, 08, 18)), Is.False, "Tuesday of the week 34");
        Assert.That (item.IsDayApplicable (D (2026, 08, 24)), Is.False, "Monday of the week 35");
      });
    }

    /// <summary>
    /// An item without any criteria applies any day
    /// </summary>
    [Test]
    public void TestNoRestriction ()
    {
      var item = new Item ();

      Assert.Multiple (() => {
        Assert.That (item.HasDayRestriction (), Is.False);
        Assert.That (item.IsDayApplicable (D (2026, 08, 18)), Is.True);
        Assert.That (item.IsWeekApplicable (D (2026, 08, 18)), Is.True);
      });
    }
  }
}
