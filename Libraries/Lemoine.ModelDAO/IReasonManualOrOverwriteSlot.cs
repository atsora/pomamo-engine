// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model representing reason slots that were manually set or that are required to be ovewritten
  /// 
  /// Only includes periods where:
  /// - Processing is false
  /// - OverwriteRequired is true OR ReasonSource is Manual
  /// </summary>
  public interface IReasonManualOrOverwriteSlot
    : IWithRange
    , IPartitionedByMachine
  {
    /// <summary>
    /// Reference to the reason
    /// </summary>
    IReason Reason { get; }

    /// <summary>
    /// Reason data in json format
    /// </summary>
    string JsonData { get; }

    /// <summary>
    /// Should the operator overwrite the reason
    /// in this reason slot ?
    /// </summary>
    bool OverwriteRequired { get; }

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
    bool ReferenceDataEquals (IReasonManualOrOverwriteSlot other);

    /// <summary>
    /// Clone the reason manual or overwrite slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IReasonManualOrOverwriteSlot Clone (UtcDateTimeRange range);
  }
}
