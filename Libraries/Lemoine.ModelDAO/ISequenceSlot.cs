// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of the
  /// analysis table operationslot
  /// that keeps a track of all the Operation Slot periods
  /// </summary>
  public interface ISequenceSlot: ISlot, IVersionable, IComparable<ISequenceSlot>, IPartitionedByMachineModule, IDisplayable
  {
    /// <summary>
    /// Reference to the sequence (may be null if no sequence is associated to the period slot)
    /// </summary>
    ISequence Sequence { get; }
    
    /// <summary>
    /// Optionally begin date/time of the next slot
    /// </summary>
    DateTime? NextBegin { get; set; }
  }
}
