// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for get lastShift
  /// </summary>
  public class LastShiftDTO
  {
    /// <summary>
    /// Day in format yyyy/MM/dd
    /// </summary>
    public string Day { get; set; }
    
    /// <summary>
    /// Shift
    /// </summary>
    public ShiftDTO Shift { get; set; }
  }
}
