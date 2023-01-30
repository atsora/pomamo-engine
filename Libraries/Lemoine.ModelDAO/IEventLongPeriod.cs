// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table EventLongPeriod
  /// </summary>
  public interface IEventLongPeriod: IEventMachine
  {
    /// <summary>
    /// Monitored machine that is associated to the event
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; }
    
    /// <summary>
    /// Machine mode that is associated to the event
    /// </summary>
    IMachineMode MachineMode { get; }
    
    /// <summary>
    /// Machine observation state that is associated to the event
    /// </summary>
    IMachineObservationState MachineObservationState { get; }
    
    /// <summary>
    /// Duration that triggered the event
    /// </summary>
    TimeSpan TriggerDuration { get; }
    
    /// <summary>
    /// Associated config
    /// 
    /// null if the config has been removed
    /// </summary>
    IEventLongPeriodConfig Config { get; set; }
  }
}
