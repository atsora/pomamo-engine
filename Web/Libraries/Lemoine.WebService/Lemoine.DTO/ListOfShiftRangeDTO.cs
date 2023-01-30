// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for list of shift slots in period.
  /// </summary>
  public class ListOfShiftRangeDTO
  {
    /// <summary>
    /// Begin of period in ISO format
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of period in ISO format
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// List of shift slots in period
    /// </summary>
    public List<ShiftRangeDTO> List { get; set; }

  }
}
