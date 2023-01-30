// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for getting cycles with in datetime range with work informations list for each of them.
  /// This is the second version which represents datetime with iso string
  /// </summary>
  public class CycleWithWorkInformationsV2DTO
  {
    /// <summary>
    /// Id of operation cycle
    /// </summary>
    public int CycleId { get; set; }
    
    /// <summary>
    /// Begin of operation cycle in IsoString
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of operation cycle IsoString
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// Estimated Begin
    /// </summary>
    public Nullable<bool> EstimatedBegin { get; set; }
    
    /// <summary>
    /// Estimated End
    /// </summary>
    public Nullable<bool> EstimatedEnd { get; set; }
    
    /// <summary>
    /// Serial number associated to operation cycle (may be null)
    /// </summary>
    public string SerialNumber { get; set; } 
    
    /// <summary>
    /// Work piece informations
    /// </summary>
    public List<WorkInformationDTO> WorkInformations { get; set; }  }
}
