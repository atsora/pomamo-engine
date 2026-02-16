// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model of table ReasonSlot without the machine mode
  /// 
  /// Analysis table where are stored all
  /// the Reason periods of a given machine.
  /// </summary>
  public interface IReasonColorSlot
    : IWithRange
    , IPartitionedByMachine
  {
    /// <summary>
    /// Does this correspond to a processing period ?
    /// </summary>
    bool Processing { get; }

    /// <summary>
    /// Reason color
    /// 
    /// An empty string may be returned, which means a transparent period
    /// </summary>
    string Color { get; }
    
    /// <summary>
    /// Should the operator overwrite the reason
    /// in this reason slot ?
    /// </summary>
    bool OverwriteRequired { get; }
    
    /// <summary>
    /// Auto-reason
    /// </summary>
    bool Auto { get; }

    /// <summary>
    /// True if the associated machine mode has a Running value and it is true
    /// </summary>
    bool Running { get; }
    
    /// <summary>
    /// True if the associated machine mode has a NotRunning value and it is true
    /// </summary>
    bool NotRunning { get; }

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
    bool ReferenceDataEquals (IReasonColorSlot other);
    
    /// <summary>
    /// Clone the reason color slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IReasonColorSlot Clone (UtcDateTimeRange range);
  }
}
