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
  public interface IMachineModeSlot
    : IWithRange
    , IPartitionedByMachine
  {
    /// <summary>
    /// Reference to the Machine mode
    /// </summary>
    IMachineMode MachineMode { get; }
    
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
    /// Reason sub-slots
    /// </summary>
    IList<IReasonSubSlot> ReasonSlots { get; }
    
    /// <summary>
    /// Machine observation state sub-slots
    /// </summary>
    IList<IMachineObservationStateSubSlot> MachineObservationStateSlots { get; }
    
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool ReferenceDataEquals (IMachineModeSlot other);
  }
}
