// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for next stop information
  /// </summary>
  public class NextStopDTO
  {
    /// <summary>
    /// Duration until next stop in seconds(can be negative)
    /// </summary>
    public int UntilNext { get; set; }
    
    /// <summary>
    /// True if next stop is an optional stop
    /// </summary>
    public bool IsOptional { get; set; }
  }
}
