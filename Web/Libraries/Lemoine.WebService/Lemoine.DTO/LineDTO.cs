// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for LineDTO
  /// </summary>
  public class LineDTO
  {
    /// <summary>
    /// Line id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// text displayed as line's label
    /// </summary>
    public string Display { get; set; }
  }
}
