// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineStateTemplateItem
  /// that associates a machine observation state to an applicable period
  /// </summary>
  public interface IMachineStateTemplateItem: IDataWithVersion, ISerializableModel
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
    /// Associated machine observation state (not null)
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }
    
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
  }
}
