// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IProductionStateSlot.
  /// </summary>
  public interface IProductionStateSlot
    : IMergeableItem<IProductionStateSlot>
    , IPartitionedByMachine
    , IWithRange
  {
    /// <summary>
    /// Reference to a Machine State Template
    /// 
    /// nullable
    /// </summary>
    IProductionState ProductionState { get; }

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
  }
}
