// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Weeks of the partitions of cncdatasample and cncdataperiod
  ///
  /// Same computation as the SQL function cncdataweekstart:
  /// a week starts at 00:00 UTC of the cut-off day.
  /// A row of these tables never crosses a week bound
  /// </summary>
  public static class CncDataWeek
  {
    /// <summary>
    /// Duration of a week
    /// </summary>
    public static readonly TimeSpan Duration = TimeSpan.FromDays (7);

    /// <summary>
    /// Start (included) of the week of a UTC date/time
    /// </summary>
    /// <param name="dateTime">UTC date/time</param>
    /// <param name="cutOffDay">first day of the week</param>
    /// <returns>UTC date/time</returns>
    public static DateTime GetWeekStart (DateTime dateTime, DayOfWeek cutOffDay)
    {
      var utc = (dateTime.Kind == DateTimeKind.Local) ? dateTime.ToUniversalTime () : dateTime;
      var days = ((int)utc.DayOfWeek - (int)cutOffDay + 7) % 7;
      return DateTime.SpecifyKind (utc.Date.AddDays (-days), DateTimeKind.Utc);
    }

    /// <summary>
    /// End (excluded) of the week of a UTC date/time
    /// </summary>
    /// <param name="dateTime">UTC date/time</param>
    /// <param name="cutOffDay">first day of the week</param>
    /// <returns>UTC date/time</returns>
    public static DateTime GetWeekEnd (DateTime dateTime, DayOfWeek cutOffDay) => GetWeekStart (dateTime, cutOffDay).Add (Duration);

    /// <summary>
    /// UTC date/time truncated to the millisecond, the precision of the cncdata tables (timestamp(3))
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>UTC date/time</returns>
    public static DateTime TruncateToMilliseconds (DateTime dateTime)
    {
      DateTime utc;
      switch (dateTime.Kind) {
      case DateTimeKind.Local:
        utc = dateTime.ToUniversalTime ();
        break;
      case DateTimeKind.Unspecified:
        utc = DateTime.SpecifyKind (dateTime, DateTimeKind.Utc);
        break;
      default:
        utc = dateTime;
        break;
      }
      return new DateTime (utc.Ticks - utc.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
    }
  }
}
