// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Fact
  /// </summary>
  public interface IMachineModuleActivity: IDataWithVersion, IPartitionedByMachineModule, IAutoSequencePeriod
  {
    /// <summary>
    /// Fact Id
    /// </summary>
    int Id { get; }
    
    // The three next properties are part of IAutoSequencePeriod
    /*
    /// <summary>
    /// Begin UTC date/time
    /// </summary>
    DateTime Begin { get; }
    
    /// <summary>
    /// End UTC date/time
    /// </summary>
    DateTime End { get; set; }
    
    /// <summary>
    /// Is the current mode an auto sequence mode ?
    /// </summary>
    bool AutoSequence { get; }
     */
    
    /// <summary>
    /// Length of the activity
    /// </summary>
    TimeSpan Length { get; }
    
    /// <summary>
    /// Reference to the machine mode
    /// </summary>
    IMachineMode MachineMode { get; set; }
    
    /// <summary>
    /// Is the machine module considered running ?
    /// </summary>
    bool Running { get; }
  }
}
