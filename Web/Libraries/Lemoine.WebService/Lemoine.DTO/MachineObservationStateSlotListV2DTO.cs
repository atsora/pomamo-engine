// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for list of machine observation state slots in period
  /// </summary>
  public class MachineObservationStateSlotListV2DTO
  {   
    /// <summary>
    /// Begin of period as ISO string
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of period as ISO string
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// List of machine observation state slots in period, in reverse chronological order
    /// </summary>
    public List<MachineObservationStateSlotV2DTO> List { get; set; }
  }
}
