// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Slot with a Day and Shift properties
  /// </summary>
  public interface ISlotWithDayShift: ISlot
  {
    /// <summary>
    /// Reference to the shift
    /// </summary>
    IShift Shift { get; set; }
    
    /// <summary>
    /// Reference to the day
    /// </summary>
    DateTime? Day { get; set; }
    
  }
}
