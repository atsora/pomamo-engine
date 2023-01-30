// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ShiftTemplateBreak
  /// that associates a break to an applicable period
  /// </summary>
  public interface IShiftTemplateBreak: IDataWithVersion, ISerializableModel
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

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
