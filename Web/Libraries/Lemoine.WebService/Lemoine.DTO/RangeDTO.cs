// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for GetRangeAround...
  /// </summary>
  public class RangeDTO
  {
    /// <summary>
    /// Range in date time
    /// </summary>
    public DateTimeRangeDTO DateTimeRange { get; set; }
    
    /// <summary>
    /// Range in system day
    /// </summary>
    public DayRangeDTO DayRange { get; set; }
    
    /// <summary>
    /// Display of Range
    /// </summary>
    public string RangeDisplay { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    public RangeDTO() {
      this.DateTimeRange = new DateTimeRangeDTO();
      this.DayRange = new DayRangeDTO();
    }
    
  }
}
