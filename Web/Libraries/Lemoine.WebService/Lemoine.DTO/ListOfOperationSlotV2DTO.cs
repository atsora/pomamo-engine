// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for list of operation slots in period (V2).
  /// </summary>
  public class ListOfOperationSlotV2DTO
  {
    /// <summary>
    /// Begin of operation cycle in ISO format
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of operation cycle in ISO format
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// List of operation slots in period, in reverse chronological order
    /// </summary>
    public List<OperationSlotV2DTO> List { get; set; }

    /// <summary>
    /// List of operation slots in period, in reverse chronological order
    /// </summary>
    public WorkInformationConfigDTO Config { get; set; }
  }
}
