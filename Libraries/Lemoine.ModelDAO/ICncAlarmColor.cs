// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model of table CncAlarmColor
  /// 
  /// For the moment, red is returned for all the cnc alarms.
  /// Later other colors, like orange may be returned.
  /// </summary>
  public interface ICncAlarmColor
    : IWithRange
    , IPartitionedByMonitoredMachine
  {
    /// <summary>
    /// Cnc alarm color
    /// </summary>
    string Color { get; }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    TimeSpan? Duration { get; }
    
    /// <summary>
    /// Is the machine module slot empty ?
    /// 
    /// If the slot is empty, it will not be inserted in the database.
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
    
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool ReferenceDataEquals (ICncAlarmColor other);
    
    /// <summary>
    /// Clone the cnc alarm color slot but with a new date/time range and duration
    /// </summary>
    /// <param name="range"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    ICncAlarmColor Clone (UtcDateTimeRange range, TimeSpan? duration);
  }
}
