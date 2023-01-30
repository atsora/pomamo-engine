// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Persistent class of table MachineActivitySummary
  /// 
  /// Analysis table in which are stored by day and
  /// Machine Observation State the time spent
  /// in each machine mode for a given machine.
  /// 
  /// It allows for example to get the total run time.
  /// </summary>
  public interface IMachineActivitySummary: IVersionable, IPartitionedByMachine
  {
    /// <summary>
    /// Day of the analysis (from cut-off time)
    /// </summary>
    DateTime Day { get; }
    
    /// <summary>
    /// Reference to the MachineObservationState
    /// </summary>
    IMachineObservationState MachineObservationState { get; }
    
    /// <summary>
    /// Reference to the machine mode
    /// </summary>
    IMachineMode MachineMode { get; }
    
    /// <summary>
    /// Reference to the shift
    /// </summary>
    IShift Shift { get; }
    
    /// <summary>
    /// Total time of the machine mode during the period
    /// </summary>
    TimeSpan Time { get; set; }
  }
}
