// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Pulse.Extensions.Database.Accumulator
{
  /// <summary>
  /// Description of IOperationCycleAccumulator.
  /// </summary>
  public interface IOperationCycleAccumulator: IAccumulator
  {
    /// <summary>
    /// An operation cycle was updated
    /// </summary>
    /// <param name="before">if not null, it must be initialized</param>
    /// <param name="after">it not null, it must be initialized</param>
    void OperationCycleUpdated (IOperationCycle before,
                                IOperationCycle after);
  }
}
