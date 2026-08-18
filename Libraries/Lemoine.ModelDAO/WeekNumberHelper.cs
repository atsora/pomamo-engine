// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Utility class to compute the week number and the week year of a specific day,
  /// according to the Global.Calendar.FirstDayOfWeek and Global.Calendar.CalendarWeekRule configurations
  /// </summary>
  public static class WeekNumberHelper
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WeekNumberHelper).FullName);

    /// <summary>
    /// Configured first day of the week (Monday by default)
    /// </summary>
    public static DayOfWeek FirstDayOfWeek => Lemoine.Info.ConfigSet
      .LoadAndGet<DayOfWeek> (ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.FirstDayOfWeek), DayOfWeek.Monday);

    /// <summary>
    /// Get the week year and the week number of the specified day
    ///
    /// Note: the week year may differ from the year of the day for the days
    /// that are at the boundary between two years
    /// </summary>
    /// <param name="day">local or unspecified day (the time part is not considered)</param>
    /// <param name="weekYear"></param>
    /// <param name="weekNumber"></param>
    public static void GetWeek (DateTime day, out int weekYear, out int weekNumber)
    {
      var firstDayOfWeek = FirstDayOfWeek;
      var calendarWeekRuleString = Lemoine.Info.ConfigSet
        .LoadAndGet<string> (ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.CalendarWeekRule), "Iso");
      var calendar = CultureInfo.CurrentCulture.Calendar;
      if (!Enum.TryParse<CalendarWeekRule> (calendarWeekRuleString, out var calendarWeekRule)) {
        if (!calendarWeekRuleString.Equals ("Iso", StringComparison.InvariantCultureIgnoreCase)) {
          log.Error ($"GetWeek: invalid week rule {calendarWeekRuleString} => use the ISO rule instead");
        }
        calendarWeekRule = CalendarWeekRule.FirstFourDayWeek;
        // .NET Standard 2.0 / .NET Framework 4.8 do not provide the ISOWeek API,
        // then cheat (see https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/):
        // if it is a Monday, a Tuesday or a Wednesday, the week number is the same
        // as the one of the following Thursday, Friday or Saturday, which is always right
        var d = day;
        if ((DayOfWeek.Monday <= d.DayOfWeek) && (d.DayOfWeek <= DayOfWeek.Wednesday)) {
          d = d.AddDays (3);
        }
        weekNumber = calendar.GetWeekOfYear (d, calendarWeekRule, firstDayOfWeek);
      }
      else {
        weekNumber = calendar.GetWeekOfYear (day, calendarWeekRule, firstDayOfWeek);
      }

      if ((1 == day.Month) && (52 <= weekNumber)) {
        weekYear = day.Year - 1;
      }
      else if ((12 == day.Month) && (1 == weekNumber)) {
        weekYear = day.Year + 1;
      }
      else {
        weekYear = day.Year;
      }
    }

    /// <summary>
    /// Get the week number of the specified day
    /// </summary>
    /// <param name="day">local or unspecified day (the time part is not considered)</param>
    /// <returns></returns>
    public static int GetWeekNumber (DateTime day)
    {
      GetWeek (day, out var _, out var weekNumber);
      return weekNumber;
    }

    /// <summary>
    /// Get the first day of the week that contains the specified day
    /// </summary>
    /// <param name="day">local or unspecified day (the time part is removed)</param>
    /// <returns>a day with the same kind as the parameter</returns>
    public static DateTime GetWeekStart (DateTime day)
    {
      var offset = (7 + (int)day.DayOfWeek - (int)FirstDayOfWeek) % 7;
      return day.Date.AddDays (-offset);
    }

    /// <summary>
    /// Get the first day of the week identified by a week year and a week number
    /// </summary>
    /// <param name="weekYear"></param>
    /// <param name="weekNumber">between 1 and 53</param>
    /// <returns>an unspecified day</returns>
    public static DateTime GetWeekStart (int weekYear, int weekNumber)
    {
      // The first week of the year always contains one of the ten first days of the year,
      // whatever the calendar week rule is
      for (var candidate = new DateTime (weekYear, 1, 1);
           candidate < new DateTime (weekYear, 1, 11);
           candidate = candidate.AddDays (1)) {
        GetWeek (candidate, out var candidateWeekYear, out var candidateWeekNumber);
        if ((candidateWeekYear == weekYear) && (1 == candidateWeekNumber)) {
          return GetWeekStart (candidate).AddDays (7 * (weekNumber - 1));
        }
      }

      log.Warn ($"GetWeekStart: the first week of {weekYear} could not be determined => fallback on the ISO rule");
      return GetWeekStart (new DateTime (weekYear, 1, 4)).AddDays (7 * (weekNumber - 1));
    }

    /// <summary>
    /// Get the number of weeks between a reference week and the week that contains the specified day
    ///
    /// The result is negative if the day is before the reference week
    /// </summary>
    /// <param name="referenceWeekYear"></param>
    /// <param name="referenceWeekNumber">between 1 and 53</param>
    /// <param name="day">local or unspecified day (the time part is not considered)</param>
    /// <returns></returns>
    public static int GetWeekDifference (int referenceWeekYear, int referenceWeekNumber, DateTime day)
    {
      var referenceWeekStart = GetWeekStart (referenceWeekYear, referenceWeekNumber);
      var weekStart = GetWeekStart (day);
      var days = (int)Math.Round ((weekStart.Date - referenceWeekStart.Date).TotalDays);
      return days / 7;
    }
  }
}
