// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for the machinemoduledetection table
  /// </summary>
  public interface IMachineModuleDetection: Lemoine.Collections.IDataWithId, IPartitionedByMachineModule
  {
    /// <summary>
    /// Date/time of the machine module detection
    /// </summary>
    DateTime DateTime { get; }
    
    /// <summary>
    /// Name of the detection
    /// 
    /// nullable
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Reference to a stamp
    /// 
    /// nullable
    /// </summary>
    IStamp Stamp { get; set; }
    
    /// <summary>
    /// Stop a NC Program
    /// </summary>
    bool StopNcProgram { get; set; }
    
    /// <summary>
    /// Start a cycle
    /// </summary>
    bool StartCycle { get; set; }
    
    /// <summary>
    /// Stop a cycle
    /// </summary>
    bool StopCycle { get; set; }
    
    /// <summary>
    /// Part quantity for the cycle
    /// </summary>
    int? Quantity { get; set; }
    
    /// <summary>
    /// Operation code
    /// </summary>
    string OperationCode { get; set; }
    
    /// <summary>
    /// Custom type
    /// </summary>
    string CustomType { get; set; }
    
    /// <summary>
    /// Custom value
    /// </summary>
    string CustomValue { get; set; }

    /// <summary>
    /// Sequence milestone
    /// </summary>
    TimeSpan? SequenceMilestone { get; set; }
  }
}
