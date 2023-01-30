// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ReasonSummary
  /// 
  /// Analysis table in which is stored a summary of the used reasons by day
  /// </summary>
  public interface IReasonSummary: IVersionable, IPartitionedByMachine
  {
    /// <summary>
    /// Day of the analysis (from cut-off time)
    /// </summary>
    DateTime Day { get; }
    
    /// <summary>
    /// Reference to the Machine Observation State
    /// </summary>
    IMachineObservationState MachineObservationState { get; }
    
    /// <summary>
    /// Reference to the Reason
    /// </summary>
    IReason Reason { get; }
    
    /// <summary>
    /// Reference to the shift
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// Total time of the machine mode during the period
    /// </summary>
    TimeSpan Time { get; set; }
    
    /// <summary>
    /// Number of reason slots during the period
    /// </summary>
    int Number { get; set; }
  }
}
