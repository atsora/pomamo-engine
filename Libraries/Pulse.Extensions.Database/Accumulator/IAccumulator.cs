// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Pulse.Extensions.Database.Accumulator
{
  /// <summary>
  /// Interface for all the accumulators
  /// </summary>
  public interface IAccumulator
  {
    /// <summary>
    /// Store the content of the accumulator in the database
    /// </summary>
    /// <param name="transactionName"></param>
    void Store (string transactionName);

    /// <summary>
    /// Is the accumulator a reason slot accumulator ?
    /// </summary>
    /// <returns></returns>
    bool IsReasonSlotAccumulator ();

    /// <summary>
    /// Is the accumulator an operation slot accumulator ?
    /// </summary>
    /// <returns></returns>
    bool IsOperationSlotAccumulator ();

    /// <summary>
    /// Is the accumulator an operation cycle accumulator ?
    /// </summary>
    /// <returns></returns>
    bool IsOperationCycleAccumulator ();

    /// <summary>
    /// Is the accumulator an observation state slot accumulator ?
    /// </summary>
    /// <returns></returns>
    bool IsObservationStateSlotAccumulator ();
  }
}
