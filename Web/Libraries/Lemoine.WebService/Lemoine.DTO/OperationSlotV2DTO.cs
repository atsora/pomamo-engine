// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// ResponseDTO for OperationSlot (V2).
  /// </summary>
  public class OperationSlotV2DTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int OperationSlotId { get; set; }
    
    /// <summary>
    /// Begin of operation slot in ISO format
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// (Opt.) End of operation slot  in ISO format
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// Work informations of operation slot
    /// </summary>
    public List<WorkInformationDTO> WorkInformations { get; set; }
    
    /// <summary>
    /// Color of operation slot for bar (default = grey)
    /// </summary>
    public string Color { get; set; }
    
  }
}
