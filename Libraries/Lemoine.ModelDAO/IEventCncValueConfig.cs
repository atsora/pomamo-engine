// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table EventCncValueConfig
  /// </summary>
  public interface IEventCncValueConfig: IVersionable
  {
    /// <summary>
    /// Config name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Event message
    /// </summary>
    string Message { get; set; }
    
    /// <summary>
    /// Associated field
    /// </summary>
    IField Field { get; }
    
    /// <summary>
    /// Optional machine filter
    /// </summary>
    IMachineFilter MachineFilter { get; set; }
    
    /// <summary>
    /// Condition to trigger the event
    /// </summary>
    string Condition { get; set; }
    
    /// <summary>
    /// Minimum duration before triggering the event
    /// </summary>
    TimeSpan MinDuration { get; set; }
    
    /// <summary>
    /// Level to associate to the event (can't be null)
    /// </summary>
    IEventLevel Level { get; set; }
  }
}
