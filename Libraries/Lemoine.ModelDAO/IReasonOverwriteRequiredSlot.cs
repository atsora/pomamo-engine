// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model representing reason slots where overwrite is required
  /// 
  /// Only includes periods where OverwriteRequired is true and Processing is false
  /// </summary>
  public interface IReasonOverwriteRequiredSlot
    : IWithRange
    , IPartitionedByMachine
  {
    /// <summary>
    /// Reason color
    /// 
    /// An empty string may be returned, which means a transparent period
    /// </summary>
    string Color { get; }

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
    bool ReferenceDataEquals (IReasonOverwriteRequiredSlot other);
    
    /// <summary>
    /// Clone the reason overwrite required slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IReasonOverwriteRequiredSlot Clone (UtcDateTimeRange range);
  }
}
