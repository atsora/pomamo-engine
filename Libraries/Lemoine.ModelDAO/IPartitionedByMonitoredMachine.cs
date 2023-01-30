// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the models that may be partioned by monitored machine
  /// </summary>
  public interface IPartitionedByMonitoredMachine
  {
    /// <summary>
    /// Associated monitored machine
    /// 
    /// Can't be null
    /// </summary>
    IMonitoredMachine Machine { get; }
  }
}
