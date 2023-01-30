// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Interface to define how to compute the offset duration of a betweencycles object
  /// </summary>
  public interface IBetweenCyclesOffsetDurationExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Priority in case different plugins implement it
    /// (in that case consider the plugin with the highest priority)
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// Compute the offset duration
    /// </summary>
    /// <param name="betweenCycles"></param>
    /// <returns></returns>
    double? ComputeOffsetDuration (IBetweenCycles betweenCycles);
  }
}
