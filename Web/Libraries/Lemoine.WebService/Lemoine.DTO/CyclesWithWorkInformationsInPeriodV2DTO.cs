// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for cycles in period
  /// This version represents datetime with iso string
  /// </summary>
  public class CyclesWithWorkInformationsInPeriodV2DTO
  {
    /// <summary>
    /// Begin of operation cycle represents in Iso string format
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of operation cycle represents in Iso string format
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// List of cycles in period, in reverse chronological order
    /// </summary>
    public List<CycleWithWorkInformationsV2DTO> List { get; set; }
  }
}
