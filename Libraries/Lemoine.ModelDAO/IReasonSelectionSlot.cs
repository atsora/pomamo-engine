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
  public interface IReasonSelectionSlot
    : IWithRange
    , IPartitionedByMachine
  {
    /// <summary>
    /// Reference to the Reason
    /// </summary>
    IReason Reason { get; }
    
    /// <summary>
    /// Running property of the machine mode
    /// </summary>
    bool Running { get; }
    
    /// <summary>
    /// Should the operator overwrite the reason
    /// in this reason slot ?
    /// </summary>
    bool OverwriteRequired { get; }
    
    /// <summary>
    /// Reason details
    /// </summary>
    string ReasonDetails { get; }
    
    /// <summary>
    /// Does it correspond to a default reason ?
    /// </summary>
    bool DefaultReason { get; }
    
    /// <summary>
    /// Set of selectable reasons
    /// </summary>
    HashSet<IReason> SelectableReasons { get; }
    
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
    /// Machine mode sub-slots
    /// </summary>
    IList<IMachineModeSubSlot> MachineModeSlots { get; }
    
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
    bool ReferenceDataEquals (IReasonSelectionSlot other);
  }
}
