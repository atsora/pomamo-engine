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
  public interface IObservationStateSlotAccumulator: IAccumulator
  {
    /// <summary>
    /// Add an observation state slot period
    /// </summary>
    /// <param name="observationStateSlot"></param>
    /// <param name="range"></param>
    void AddObservationStateSlotPeriod (IObservationStateSlot observationStateSlot,
                                        UtcDateTimeRange range);
    
    /// <summary>
    /// Remove an observation state slot period
    /// </summary>
    /// <param name="observationStateSlot"></param>
    /// <param name="range"></param>
    void RemoveObservationStateSlotPeriod (IObservationStateSlot observationStateSlot,
                                           UtcDateTimeRange range);
  }
}
