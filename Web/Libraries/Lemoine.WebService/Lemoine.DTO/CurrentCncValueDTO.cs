// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for GetCurrentCncValue
  /// </summary>
  public class CurrentCncValueDTO
  {
    /// <summary>
    /// Unit of cnc value field
    /// </summary>
    public string Unit { get; set; }
    
    /// <summary>
    /// Last ISO date when cnc value was seen
    /// May be null (then LastCncValueData also null)
    /// </summary>
    public string LastCncValueDate { get; set; }
    
    /// <summary>
    /// Last seen value of cnc value as string
    /// May be null (then LastCncValueDate also null)
    /// </summary>
    public string LastCncValueData { get; set; }    
  }
}
