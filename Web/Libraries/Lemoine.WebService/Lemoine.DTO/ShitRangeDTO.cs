// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// output DTO for GetShiftAround / GetShiftBefore / GetShiftAfter.
  /// </summary>
  public class ShiftRangeDTO
  {
    /// <summary>
    /// Display of the shift
    /// </summary>
    public string ShiftDisplay { get; set; }
    
    /// <summary>
    /// Begin of the shift (ISO string)
    /// </summary>
    public string ShiftBegin { get; set; }
    
    /// <summary>
    /// End of the shift (ISO string)
    /// </summary>
    public string ShiftEnd { get; set; }
    
    /// <summary>
    /// Color of shift
    /// </summary>
    public string ShiftColor { get; set; }
  }
}
