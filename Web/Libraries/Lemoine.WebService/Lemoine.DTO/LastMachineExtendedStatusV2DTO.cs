// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for LastMachineExtendedStatus(V2).
  /// </summary>
  public class LastMachineExtendedStatusV2DTO
  {
    /// <summary>
    /// Machine status
    /// </summary>
    public MachineExtendedStatusV2DTO MachineStatus { get; set; }
    
    /// <summary>
    /// Required reason in the past hours ?
    /// </summary>
    public bool RequiredReason { get; set; }

    /// <summary>
    /// Number of reasons that require to be overridden
    /// </summary>
    public int RequiredNumber { get; set; }
    
    /// <summary>
    /// Reason is too old?
    /// </summary>
    public bool ReasonTooOld { get; set; }
  }
}
