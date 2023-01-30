// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Pulse.Extensions.Database.Accumulator
{
  /// <summary>
  /// Description of IOperationSlotAccumulator.
  /// </summary>
  public interface IOperationSlotAccumulator: IAccumulator
  {
    /// <summary>
    /// An operation slot was updated
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after">not null</param>
    void OperationSlotUpdated (IOperationSlot before,
                               IOperationSlot after);
    
    /// <summary>
    /// An operation slot was removed
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <param name="initialState">Initial state without the ID</param>
    void OperationSlotRemoved (IOperationSlot operationSlot,
                               IOperationSlot initialState);
  }
}
