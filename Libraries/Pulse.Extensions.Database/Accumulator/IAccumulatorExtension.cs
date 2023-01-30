// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;

namespace Pulse.Extensions.Database.Accumulator
{
  /// <summary>
  /// Accumulator extension
  /// </summary>
  public interface IAccumulatorExtension
    : IExtension
  {
    /// <summary>
    /// To define the order in which the accumulators are processed
    /// 
    /// Use the following ranges:
    /// <item>100-199: IReasonSlotAccumulators</item>
    /// <item>200-299: IOperationSlotAccumulators</item>
    /// <item>300-399: both IOperationSlotAccumulators and IOperationCycleAccumulators</item>
    /// <item>400-499: IOperationCycleAccumulators</item>
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <returns>if false is returned, skip this extension (not active)</returns>
    bool Initialize ();

    /// <summary>
    /// Create an accumulator
    /// </summary>
    /// <returns></returns>
    IAccumulator Create ();
  }
}
