// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// RangeDTO: combination of DateTimeRangeDTO and DayRangeDTO that includes a display as well
  /// </summary>
  public class RangeDTO
  {
    /// <summary>
    /// Range in date time
    /// </summary>
    public string DateTimeRange { get; set; }
    
    /// <summary>
    /// Range in system day
    /// </summary>
    public string DayRange { get; set; }
    
    /// <summary>
    /// Display of Range
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    public RangeDTO()
    { }
  }
}
