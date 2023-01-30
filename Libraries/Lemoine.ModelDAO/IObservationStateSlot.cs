// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ObservationStateSlot
  /// 
  /// Analysis table where are stored all
  /// the Machine Observation State periods of a given machine.
  /// </summary>
  public interface IObservationStateSlot
    : ISlotWithDayShift
    , IPartitionedByMachine
    , IDataWithVersion
    , IComparable<IObservationStateSlot>
    , IWithTemplate
  {
    /// <summary>
    /// Reference to the Machine Observation State
    /// 
    /// not null if MachineStateTemplate is null
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }

    /// <summary>
    /// Reference to a Machine State Template
    /// 
    /// nullable
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; set; }

    /// <summary>
    /// Reference to the User
    /// 
    /// nullable
    /// </summary>
    IUser User { get; set; }

    /// <summary>
    /// Does this slot correspond to a production ?
    /// </summary>
    bool? Production { get; set; }
  }

  /// <summary>
  /// Extensions to interface <see cref="IObservationStateSlot"/>
  /// </summary>
  public static class ObservationStateSlotExtensions
  {
    /// <summary>
    /// Does an IObservationStateSlot correspond to a production ?
    /// 
    /// true if Production has a value and is true
    /// </summary>
    /// <returns></returns>
    public static bool IsProduction (this IObservationStateSlot slot)
    {
      return slot.Production.HasValue && slot.Production.Value;
    }
  }
}
