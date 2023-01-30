// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for MachineExtendedStatus (V2)
  /// </summary>
  public class MachineExtendedStatusV2DTO
  {
    /// <summary>
    /// Reason slot
    /// </summary>
    public ReasonSlotV2DTO ReasonSlot { get; set; }
    
    /// <summary>
    /// MachineObservationState
    /// </summary>
    public MachineObservationStateDTO MachineObservationState { get; set; }
    
    /// <summary>
    /// Machine mode
    /// </summary>
    public MachineModeDTO MachineMode { get; set; }
  }
}
