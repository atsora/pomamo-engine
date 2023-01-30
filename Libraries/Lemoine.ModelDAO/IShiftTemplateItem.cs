// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ShiftTemplateItem
  /// that associates a shift to an applicable period
  /// </summary>
  public interface IShiftTemplateItem: IDataWithVersion, ISerializableModel
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Associated shift (not null)
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
  }
}
