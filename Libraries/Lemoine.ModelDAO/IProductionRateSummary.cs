// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ProductionRateSummary
  /// 
  /// Analysis table in which is stored a summary of the production rates by day and shift
  /// </summary>
  public interface IProductionRateSummary : IVersionable, IPartitionedByMachine
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
    /// Total time when a production rate is set during the period
    /// </summary>
    TimeSpan Duration { get; set; }

    /// <summary>
    /// Average production rate during this period
    /// </summary>
    double ProductionRate { get; set; }
  }
}
