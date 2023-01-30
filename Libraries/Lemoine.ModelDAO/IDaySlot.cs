// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Persistent class of analysis table dayslot
  /// </summary>
  public interface IDaySlot: ISlotWithDayShift, IComparable<IDaySlot>, IDataWithVersion
  {
    /// <summary>
    /// Reference to a day template
    /// </summary>
    IDayTemplate DayTemplate { get; set; }

    /// <summary>
    /// Year associated to the week number
    /// </summary>
    int? WeekYear { get; }

    /// <summary>
    /// Week number (in association with the WeekYear property)
    /// </summary>
    int? WeekNumber { get; }
  }
}
