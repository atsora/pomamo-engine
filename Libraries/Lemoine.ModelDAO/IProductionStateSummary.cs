// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ProductionStateSummary
  /// 
  /// Analysis table in which is stored a summary of the applied production states by day and shift
  /// </summary>
  public interface IProductionStateSummary : IVersionable, IPartitionedByMachine
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
    /// Reference to the shift
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// Reference to the ProductionState
    /// </summary>
    IProductionState ProductionState { get; }

    /// <summary>
    /// Total time of the production state during the period
    /// </summary>
    TimeSpan Duration { get; set; }
  }
}
