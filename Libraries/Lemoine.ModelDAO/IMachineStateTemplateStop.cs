// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineStateTemplateStop
  /// that returns the time when a machine observation state must be interrupted
  /// </summary>
  public interface IMachineStateTemplateStop: IDataWithVersion, ISerializableModel
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
    /// 
    /// Fraction of the day that has elapsed since local midnight
    /// </summary>
    TimeSpan? LocalTime { get; set; }
  }
}
