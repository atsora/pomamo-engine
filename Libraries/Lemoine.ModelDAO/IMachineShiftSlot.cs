// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual model of table observationstateslot with only the shift
  /// </summary>
  public interface IMachineShiftSlot
    : IMergeableItem<IMachineShiftSlot>
    , IPartitionedByMachine
    , IWithRange
  {
    /// <summary>
    /// Reference to a shift
    /// 
    /// nullable
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// Duration of the slot
    /// </summary>
    TimeSpan? Duration { get; }

    /// <summary>
    /// Is the machine slot empty ? Empty if Shift is null
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
  }
}
