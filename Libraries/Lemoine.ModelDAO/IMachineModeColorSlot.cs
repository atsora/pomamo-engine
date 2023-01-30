// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model of table ReasonSlot with only the machine mode
  /// </summary>
  public interface IMachineModeColorSlot
    : IWithRange
    , IPartitionedByMachine
  {
    /// <summary>
    /// Machine mode color
    /// </summary>
    string Color { get; }
    
    /// <summary>
    /// True if Machine mode has a running value and it is true
    /// </summary>
    bool Running { get; }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    TimeSpan? Duration { get; }
    
    /// <summary>
    /// Is the machine slot empty ?
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
    bool ReferenceDataEquals (IMachineModeColorSlot other);
    
    /// <summary>
    /// Clone the machine mode color slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IMachineModeColorSlot Clone (UtcDateTimeRange range);
  }
}
