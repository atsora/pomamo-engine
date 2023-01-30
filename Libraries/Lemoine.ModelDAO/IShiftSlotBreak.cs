// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ShiftSlotBreak
  /// that returns a break that is associated to a shift slot
  /// </summary>
  public interface IShiftSlotBreak
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }
    
    /// <summary>
    /// UTC Date/time range of the break
    /// </summary>
    UtcDateTimeRange Range { get; }
  }
}
