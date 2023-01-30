// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineModuleAnalysisStatus
  /// 
  /// This new table stores the current analysis status of a machine module
  /// </summary>
  public interface IMachineModuleAnalysisStatus: IVersionable
  {
    /// <summary>
    /// Reference to the MachineModule
    /// 
    /// not null
    /// </summary>
    IMachineModule MachineModule { get; }
    
    /// <summary>
    /// Last processed Id of MachineModuleDetection
    /// </summary>
    int LastMachineModuleDetectionId { get; set; }
    
    /// <summary>
    /// UTC Date/time of the auto-sequence analysis
    /// </summary>
    DateTime AutoSequenceAnalysisDateTime { get; set; }
  }
}
