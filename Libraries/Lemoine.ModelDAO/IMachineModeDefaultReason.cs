// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table MachineModeDefaultReason
  /// 
  /// The default reason that is associated to a machine mode
  /// for a given machine observation state is defined in this table.
  /// </summary>
  public interface IMachineModeDefaultReason : IVersionable
  {
    /// <summary>
    /// Reference to the Machine Mode
    /// 
    /// Not null
    /// </summary>
    IMachineMode MachineMode { get; }

    /// <summary>
    /// Reference to the Machine Observation State
    /// 
    /// Not null
    /// </summary>
    IMachineObservationState MachineObservationState { get; }

    /// <summary>
    /// Include a set of machine from the configuration with the help of a IMachineFilter
    /// </summary>
    IMachineFilter IncludeMachineFilter { get; set; }

    /// <summary>
    /// Exclude a set of machine from the configuration with the help of a IMachineFilter
    /// </summary>
    IMachineFilter ExcludeMachineFilter { get; set; }

    /// <summary>
    /// Maximum duration in seconds. If null, there is no limitation
    /// </summary>
    Nullable<TimeSpan> MaximumDuration { get; set; }

    /// <summary>
    /// Reference to the default Reason
    /// 
    /// Not null
    /// </summary>
    IReason Reason { get; set; }

    /// <summary>
    /// Consider this is an auto reason
    /// </summary>
    bool Auto { get; set; }

    /// <summary>
    /// Reason score
    /// </summary>
    double Score { get; set; }

    /// <summary>
    /// If TRUE, the operator must assign a new real reason to this period
    /// </summary>
    bool OverwriteRequired { get; set; }

    /// <summary>
    /// Check if this default reason is applicable to the machine
    /// 
    /// Note: IncludeMachineFilter and ExcludeMachineFilter must be known,
    ///       so you probably want to run it inside a session
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    bool IsApplicableToMachine (IMachine machine);
  }
}
