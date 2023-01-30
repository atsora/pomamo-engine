// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// IEventMachine: IEvent for a specific machine module
  /// </summary>
  public interface IEventMachineModule: IEventMachine, IPartitionedByMachineModule
  {
    /// <summary>
    /// Monitored machine that is associated to the event
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; }
  }
}
