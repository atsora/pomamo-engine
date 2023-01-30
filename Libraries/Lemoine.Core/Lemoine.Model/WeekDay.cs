// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.I18N;

namespace Lemoine.Model
{
  /// <summary>
  /// Week days
  /// </summary>
  [Flags]
  public enum WeekDay
  {
    /// <summary>
    /// No week day
    /// </summary>
    None = 0,
    /// <summary>
    /// Monday
    /// </summary>
    Monday = 1,
    /// <summary>
    /// Tuesday
    /// </summary>
    Tuesday = 2, // 1 << 1
    /// <summary>
    /// Wednesday
    /// </summary>
    Wednesday = 4, // 1 << 2
    /// <summary>
    /// Thursday
    /// </summary>
    Thursday = 8, // 1 << 3
    /// <summary>
    /// Friday
    /// </summary>
    Friday = 16, // 1 << 4
    /// <summary>
    /// Saturday
    /// </summary>
    Saturday = 32, // 1 << 5
    /// <summary>
    /// Sunday
    /// </summary>
    Sunday = 64, // 1 << 6
    /// <summary>
    /// All week days
    /// </summary>
    AllDays = 127
  }

  /// <summary>
  /// Extensions to DayOfWeek:
  /// <item>to convert it to the type WeekDay</item>
  /// </summary>
  public static class DayOfWeekExtensions
  {
    /// <summary>
    /// Convert a DayOfWeek type to a WeekDay
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <returns></returns>
    public static WeekDay ConvertToWeekDay (this DayOfWeek dayOfWeek)
    {
      switch (dayOfWeek) {
      case DayOfWeek.Sunday:
        return WeekDay.Sunday;
      case DayOfWeek.Monday:
        return WeekDay.Monday;
      case DayOfWeek.Tuesday:
        return WeekDay.Tuesday;
      case DayOfWeek.Wednesday:
        return WeekDay.Wednesday;
      case DayOfWeek.Thursday:
        return WeekDay.Thursday;
      case DayOfWeek.Friday:
        return WeekDay.Friday;
      case DayOfWeek.Saturday:
        return WeekDay.Saturday;
      default:
        throw new Exception ("Invalid value for DayOfWeek");
      }
    }
  }

  /// <summary>
  /// Extensions to WeekDay:
  /// <item>check if it matches another WeekDay</item>
  /// <item>check if it matches a DayOfWeek</item>
  /// <item>ToString() with i18n support</item>
  /// </summary>
  public static class WeekDayExtensions
  {
    /// <summary>
    /// Check if t includes other
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlagDayOfWeek (this WeekDay t, DayOfWeek other)
    {
      WeekDay o = other.ConvertToWeekDay ();
      return t.HasFlag (o);
    }

    /// <summary>
    /// Return a translated ToString version
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string ToString (this WeekDay t)
    {
      String[] weekDayStr = new String[8];

      if (t.HasFlag (WeekDay.AllDays)) {
        weekDayStr[0] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.AllDays));
        return weekDayStr[0];
      }

      if (t.HasFlag (WeekDay.Monday)) {
        weekDayStr[1] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.Monday));
      }

      if (t.HasFlag (WeekDay.Tuesday)) {
        weekDayStr[2] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.Tuesday));
      }

      if (t.HasFlag (WeekDay.Wednesday)) {
        weekDayStr[3] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.Wednesday));
      }

      if (t.HasFlag (WeekDay.Thursday)) {
        weekDayStr[4] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.Thursday));
      }

      if (t.HasFlag (WeekDay.Friday)) {
        weekDayStr[5] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.Friday));
      }

      if (t.HasFlag (WeekDay.Saturday)) {
        weekDayStr[6] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.Saturday));
      }

      if (t.HasFlag (WeekDay.Sunday)) {
        weekDayStr[7] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.Sunday));
      }

      if (weekDayStr.Length < 1) //empty
{
        weekDayStr[0] = PulseCatalog.GetString (Enum.GetName (typeof (WeekDay), WeekDay.None));
      }

      return String.Join (",", weekDayStr);
    }

  }
}
