// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Pulse.Extensions.Database.Accumulator
{
  /// <summary>
  /// Interface for all the summary accumulators
  /// </summary>
  public interface IReasonSlotAccumulator: IAccumulator
  {
    /// <summary>
    /// Add a positive or negative duration for the specified reason slot and day
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="day"></param>
    /// <param name="duration"></param>
    void AddReasonSlotDuration (IReasonSlot reasonSlot,
                                DateTime day,
                                TimeSpan duration);

    /// <summary>
    /// Add a positive or negative number of slots for the specified reason slot and day
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="day"></param>
    /// <param name="number"></param>
    void AddReasonSlotNumber (IReasonSlot reasonSlot,
                              DateTime day,
                              int number);
  }
}
