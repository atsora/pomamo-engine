// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for MachineObservationStateSlotV2.
  /// </summary>
  public class MachineObservationStateSlotV2DTO
  {
    /// <summary>
    /// Id of slot
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Begin of observation slot as offset in ms w.r.t. 1970/1/1
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// (Opt.) End of observation slot as offset in ms w.r.t. 1970/1/1
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// Machine Observation State
    /// </summary>
    public MachineObservationStateDTO MachineObservationState { get; set; }
        
  }
}
