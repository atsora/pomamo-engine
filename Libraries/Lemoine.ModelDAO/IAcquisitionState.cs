// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// List of common acquisition state keys
  /// </summary>
  public enum AcquisitionStateKey
  {
    /// <summary>
    /// Acquisition of stamps, sequences, operations
    /// </summary>
    Detection = 1,

    /// <summary>
    /// Acquisition of alarms
    /// </summary>
    Tools = 2,

    /// <summary>
    /// Acquisition of alarms
    /// </summary>
    Alarms = 3,
  }
  
  /// <summary>
  /// Description of IAcquisitionStatus.
  /// </summary>
  public interface IAcquisitionState: IVersionable, IPartitionedByMachineModule
  {
    /// <summary>
    /// Type of acquisition
    /// 
    /// Not null or empty
    /// </summary>
    AcquisitionStateKey Key { get; }

    /// <summary>
    /// Last acquisition datetime
    /// </summary>
    DateTime DateTime { get; set; }
  }
}
