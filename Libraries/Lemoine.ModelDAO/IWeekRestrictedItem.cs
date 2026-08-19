// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Item of a template that may be restricted to one specific week,
  /// possibly repeated every x weeks
  /// </summary>
  public interface IWeekRestrictedItem
  {
    /// <summary>
    /// Year of the applicable specific week (nullable)
    ///
    /// It is only considered when <see cref="WeekNumber"/> is set.
    /// When it is not set, the week number applies to any year
    /// </summary>
    int? WeekYear { get; set; }

    /// <summary>
    /// Number of the applicable specific week, between 1 and 53 (nullable)
    ///
    /// The week number is computed with the Global.Calendar.CalendarWeekRule
    /// and Global.Calendar.FirstDayOfWeek configurations (ISO 8601 by default)
    /// </summary>
    int? WeekNumber { get; set; }

    /// <summary>
    /// Repeat the item every <see cref="WeekFrequency"/> weeks, starting from the week
    /// that is defined by <see cref="WeekYear"/> and <see cref="WeekNumber"/> (nullable)
    ///
    /// 1 means every week, 2 every two weeks, ...
    ///
    /// It is only considered when both <see cref="WeekYear"/> and <see cref="WeekNumber"/> are set.
    /// When it is not set, only the specified week is considered
    /// </summary>
    int? WeekFrequency { get; set; }
  }

  /// <summary>
  /// Extensions to <see cref="IWeekRestrictedItem"/>
  /// </summary>
  public static class IWeekRestrictedItemExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IWeekRestrictedItemExtensions).FullName);

    /// <summary>
    /// Is the specified week compatible with the week criteria of the item ?
    /// </summary>
    /// <param name="item">not null</param>
    /// <param name="weekYear"></param>
    /// <param name="weekNumber">between 1 and 53</param>
    /// <param name="yearlyRepeat">consider the week number of any year</param>
    /// <returns></returns>
    public static bool IsWeekApplicable (this IWeekRestrictedItem item, int weekYear, int weekNumber, bool yearlyRepeat = false)
    {
      if (item is null) {
        log.Fatal ("IsWeekApplicable: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      if (!item.WeekNumber.HasValue) { // No week criteria
        return true;
      }

      if (yearlyRepeat || !item.WeekYear.HasValue) {
        // The same week number is considered every year
        return weekNumber == item.WeekNumber.Value;
      }

      var weekDifference = WeekNumberHelper
        .GetWeekDifference (item.WeekYear.Value, item.WeekNumber.Value, weekYear, weekNumber);
      if (weekDifference < 0) { // Before the reference week
        return false;
      }
      if (!item.WeekFrequency.HasValue) { // Only the reference week
        return 0 == weekDifference;
      }
      if (item.WeekFrequency.Value <= 0) {
        log.Error ($"IsWeekApplicable: invalid week frequency {item.WeekFrequency.Value} => consider the reference week only");
        return 0 == weekDifference;
      }
      return 0 == (weekDifference % item.WeekFrequency.Value);
    }

    /// <summary>
    /// Is the week of the specified day compatible with the week criteria of the item ?
    /// </summary>
    /// <param name="item">not null</param>
    /// <param name="localDay">local day</param>
    /// <param name="yearlyRepeat">consider the week number of any year</param>
    /// <returns></returns>
    public static bool IsWeekApplicable (this IWeekRestrictedItem item, DateTime localDay, bool yearlyRepeat = false)
    {
      if (item is null) {
        log.Fatal ("IsWeekApplicable: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      if (!item.WeekNumber.HasValue) { // No week criteria, no need to compute the week
        return true;
      }

      WeekNumberHelper.GetWeek (localDay, out var weekYear, out var weekNumber);
      return item.IsWeekApplicable (weekYear, weekNumber, yearlyRepeat);
    }

    /// <summary>
    /// Key to sort the items by increasing reference week
    ///
    /// The items with no week criteria come first, then the ones that apply to a week number
    /// of any year, then the ones with a reference week, by increasing year and week number
    /// </summary>
    /// <param name="item">not null</param>
    /// <returns></returns>
    public static (int, int, int) GetWeekSortKey (this IWeekRestrictedItem item)
    {
      if (item is null) {
        log.Fatal ("GetWeekSortKey: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      if (!item.WeekNumber.HasValue) {
        return (0, 0, 0);
      }
      if (!item.WeekYear.HasValue) {
        return (1, 0, item.WeekNumber.Value);
      }
      return (2, item.WeekYear.Value, item.WeekNumber.Value);
    }
  }
}
