// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Output DTO to report some data on a shift
  /// </summary>
  public class ShiftDTO
  {
    /// <summary>
    /// Shift Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Shift display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Color of the shift
    /// </summary>
    public string Color { get; set; }
  }
}
