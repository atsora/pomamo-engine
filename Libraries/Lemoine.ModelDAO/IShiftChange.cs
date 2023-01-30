// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ShiftChange
  /// 
  /// This modification table records any change on a shift
  /// </summary>
  public interface IShiftChange: IModification
  {
    /// <summary>
    /// Reference to a shift
    /// </summary>
    IShift Shift { get; }
    
    /// <summary>
    /// Begin UTC date/time of a shift change
    /// </summary>
    DateTime Begin { get; }
    
    /// <summary>
    /// End UTC date/time of a shift change
    /// </summary>
    Nullable<DateTime> End { get; set; }
  }
}
