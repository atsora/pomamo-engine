// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ShiftTemplateItem
  /// that associates a shift to an applicable period
  /// </summary>
  public interface IShiftTemplateItem: IDataWithVersion, ISerializableModel, IWeekRestrictedItem
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Associated shift
    ///
    /// It must not be null, except when <see cref="SubShiftTemplate"/> is set
    /// </summary>
    IShift Shift { get; set; }

    /// <summary>
    /// Associated shift template to apply recursively (nullable)
    ///
    /// When it is set, <see cref="Shift"/> is not considered: the items of the referenced
    /// shift template are applied on the periods that are applicable for this item
    ///
    /// Note: a shift template must not reference itself, directly or indirectly
    /// </summary>
    IShiftTemplate SubShiftTemplate { get; set; }

    /// <summary>
    /// Applicable week days
    /// </summary>
    WeekDay WeekDays { get; set; }

    /// <summary>
    /// Applicable time period of day
    /// </summary>
    TimePeriodOfDay TimePeriod { get; set; }

    /// <summary>
    /// Applicable specific day
    /// </summary>
    DateTime? Day { get; set; }
  }

  /// <summary>
  /// Extensions to <see cref="IShiftTemplateItem"/>
  /// </summary>
  public static class IShiftTemplateItemExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IShiftTemplateItemExtensions).FullName);

    /// <summary>
    /// Priority of an item, the items with the highest priority overriding the other ones:
    /// <item>0: no specific week and no specific day</item>
    /// <item>1: a specific week</item>
    /// <item>2: a specific day</item>
    /// </summary>
    /// <param name="item">not null</param>
    /// <returns></returns>
    public static int GetPriority (this IShiftTemplateItem item)
    {
      if (item is null) {
        log.Fatal ("GetPriority: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      if (item.Day.HasValue) {
        return 2;
      }
      if (item.WeekNumber.HasValue) {
        return 1;
      }
      return 0;
    }

    /// <summary>
    /// Is the item applicable on the specified day, considering the day, week day
    /// and week criteria ? The time period of day is not considered here
    /// </summary>
    /// <param name="item">not null</param>
    /// <param name="localDate">local date</param>
    /// <param name="weekYear">week year of the associated day slot</param>
    /// <param name="weekNumber">week number of the associated day slot</param>
    /// <returns></returns>
    public static bool IsDayApplicable (this IShiftTemplateItem item, DateTime localDate, int weekYear, int weekNumber)
    {
      if (item is null) {
        log.Fatal ("IsDayApplicable: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      if (item.Day.HasValue) {
        // Note: the week days are not considered when a specific day is set
        if (!item.Day.Value.Date.Equals (localDate.Date)) {
          return false;
        }
      }
      else if (!item.WeekDays.HasFlagDayOfWeek (localDate.DayOfWeek)) {
        return false;
      }

      return item.IsWeekApplicable (weekYear, weekNumber);
    }
  }
}
