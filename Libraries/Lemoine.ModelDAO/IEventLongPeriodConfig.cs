// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table EventLongPeriodConfig
  /// </summary>
  public interface IEventLongPeriodConfig: IVersionable
  {
    /// <summary>
    /// Associated monitored machine
    /// 
    /// This may be null (whichever machine)
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; set; }
    
    /// <summary>
    /// Associated machine mode
    /// 
    /// It may be null (whichever machine mode)
    /// </summary>
    IMachineMode MachineMode { get; set; }
    
    /// <summary>
    /// Associated machine observation state
    /// 
    /// It may be null (whichever machine observation state)
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }
    
    /// <summary>
    /// Minimum duration of the period that triggers the event
    /// </summary>
    TimeSpan TriggerDuration { get; set; }
    
    /// <summary>
    /// Level to associate to the event (can't be null)
    /// </summary>
    IEventLevel Level { get; set; }
  }
}
