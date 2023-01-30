// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table DayTemplateChange
  /// 
  /// This modification table records any day template change
  /// </summary>
  public interface IDayTemplateChange: IGlobalModification
  {
    /// <summary>
    /// Reference to a day template
    /// </summary>
    IDayTemplate DayTemplate { get; }
    
    /// <summary>
    /// Begin UTC date/time of a shift change
    /// </summary>
    LowerBound<DateTime> Begin { get; }
    
    /// <summary>
    /// End UTC date/time of a shift change
    /// </summary>
    UpperBound<DateTime> End { get; set; }
    
    /// <summary>
    /// Force re-building the day in case there is no day template change
    /// 
    /// Default is False
    /// </summary>
    bool Force { get; set; }
  }
}
