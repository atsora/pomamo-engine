// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for last work information (V3)
  /// </summary>
  public class LastWorkInformationV3DTO
  {
    /// <summary>
    /// Is there an operation slot ?
    /// </summary>
    public bool SlotMissing { get; set; }

    /// <summary>
    /// Begin of operation slot (if it exists) in ISO format
    /// </summary>
    public string Begin { get; set; }

    /// <summary>
    /// End of operation slot (if it exists) in ISO format
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// Work piece informations
    /// </summary>
    public List<WorkInformationDTO> WorkInformations { get; set; }
    
    /// <summary>
    /// Data Missing
    /// </summary>
    public bool DataMissing { get; set; }
    
    /// <summary>
    /// Config
    /// </summary>
    public WorkInformationConfigDTO Config { get; set; }
    
  }
}
