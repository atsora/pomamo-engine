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
  public class DayRangeDTO
  {
    /// <summary>
    /// Begin DAY of range as ISO string
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End DAY of range as ISO string
    /// </summary>
    public string End { get; set; }
  }
}
