// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Persistent class of analysis table shifttemplateslot
  /// that keeps a track of all the shift template periods
  /// </summary>
  public interface IShiftTemplateSlot: ISlot, IComparable<IShiftTemplateSlot>
  {
    /// <summary>
    /// Reference to a shift template
    /// </summary>
    IShiftTemplate ShiftTemplate { get; set; }
  }
}
