// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineStateTemplateItem
  /// that associates a machine observation state to an applicable period
  /// </summary>
  public interface IMachineStateTemplateItem: IDataWithVersion, ISerializableModel, IWeekRestrictedItem
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Order in the list of items
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Associated machine observation state
    ///
    /// It must not be null, except when <see cref="SubMachineStateTemplate"/> is set
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }

    /// <summary>
    /// Associated machine state template to apply recursively (nullable)
    ///
    /// When it is set, <see cref="MachineObservationState"/> is not considered:
    /// the items of the referenced machine state template are applied one after the other
    /// on the period that is applicable for this item
    ///
    /// Note: a machine state template must not reference itself, directly or indirectly
    /// </summary>
    IMachineStateTemplate SubMachineStateTemplate { get; set; }

    /// <summary>
    /// Associated shift (nullable)
    /// </summary>
    IShift Shift { get; set; }

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

    /// <summary>
    /// Repeat the item every year
    ///
    /// When <see cref="Day"/> is set, the item applies every year on the same month and day.
    /// This is mainly useful for the public holidays.
    ///
    /// When <see cref="WeekNumber"/> is set, the item applies every year on the same week number
    /// (<see cref="WeekYear"/> and <see cref="WeekFrequency"/> are then not considered)
    /// </summary>
    bool YearlyRepeat { get; set; }
  }

  /// <summary>
  /// Extensions to <see cref="IMachineStateTemplateItem"/>
  /// </summary>
  public static class IMachineStateTemplateItemExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IMachineStateTemplateItemExtensions).FullName);

    /// <summary>
    /// Does this item restrict the days on which it is applicable ?
    ///
    /// If false, the item is applicable any day (but possibly not the whole day,
    /// see <see cref="IMachineStateTemplateItem.TimePeriod"/>)
    /// </summary>
    /// <param name="item">not null</param>
    /// <returns></returns>
    public static bool HasDayRestriction (this IMachineStateTemplateItem item)
    {
      if (item is null) {
        log.Fatal ("HasDayRestriction: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      return item.Day.HasValue
        || item.WeekNumber.HasValue
        || !item.WeekDays.HasFlag (WeekDay.AllDays);
    }

    /// <summary>
    /// Is the week of the specified day compatible with the week criteria of the item ?
    ///
    /// Unlike <see cref="IWeekRestrictedItemExtensions.IsWeekApplicable(IWeekRestrictedItem, DateTime, bool)"/>,
    /// <see cref="IMachineStateTemplateItem.YearlyRepeat"/> is taken into account
    /// </summary>
    /// <param name="item">not null</param>
    /// <param name="localDay">local day</param>
    /// <returns></returns>
    public static bool IsWeekApplicable (this IMachineStateTemplateItem item, DateTime localDay)
    {
      if (item is null) {
        log.Fatal ("IsWeekApplicable: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      return ((IWeekRestrictedItem)item).IsWeekApplicable (localDay, item.YearlyRepeat);
    }

    /// <summary>
    /// Is the item applicable on the specified day, considering the day, week day
    /// and week criteria ? The time period of day is not considered here
    /// </summary>
    /// <param name="item">not null</param>
    /// <param name="localDay">local day</param>
    /// <returns></returns>
    public static bool IsDayApplicable (this IMachineStateTemplateItem item, DateTime localDay)
    {
      if (item is null) {
        log.Fatal ("IsDayApplicable: item is null");
        throw new ArgumentNullException (nameof (item));
      }

      if (item.Day.HasValue) {
        // Note: the week days are not considered when a specific day is set
        if (item.YearlyRepeat) {
          if ((item.Day.Value.Month != localDay.Month) || (item.Day.Value.Day != localDay.Day)) {
            return false;
          }
        }
        else if (!item.Day.Value.Date.Equals (localDay.Date)) {
          return false;
        }
      }
      else if (!item.WeekDays.HasFlagDayOfWeek (localDay.DayOfWeek)) {
        return false;
      }

      return item.IsWeekApplicable (localDay);
    }

    /// <summary>
    /// Get the day of the specified year that corresponds to <see cref="IMachineStateTemplateItem.Day"/>
    /// when <see cref="IMachineStateTemplateItem.YearlyRepeat"/> is set
    /// </summary>
    /// <param name="item">not null, with Day set</param>
    /// <param name="year"></param>
    /// <returns>null if the day does not exist this year (February 29th of a non-leap year)</returns>
    public static DateTime? GetYearlyDay (this IMachineStateTemplateItem item, int year)
    {
      if (item is null) {
        log.Fatal ("GetYearlyDay: item is null");
        throw new ArgumentNullException (nameof (item));
      }
      if (!item.Day.HasValue) {
        log.Fatal ($"GetYearlyDay: no day in item {item.Id}");
        throw new InvalidOperationException ("No day in the machine state template item");
      }

      var day = item.Day.Value;
      if ((2 == day.Month) && (29 == day.Day) && !DateTime.IsLeapYear (year)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetYearlyDay: February 29th does not exist in {year}");
        }
        return null;
      }
      return new DateTime (year, day.Month, day.Day, 0, 0, 0, day.Kind);
    }
  }
}
