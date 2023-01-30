// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Persistent class of analysis table daytemplateslot
  /// that keeps a track of all the day template periods
  /// </summary>
  public interface IDayTemplateSlot: ISlot, IComparable<IDayTemplateSlot>
  {
    /// <summary>
    /// Reference to a day template
    /// </summary>
    IDayTemplate DayTemplate { get; set; }
  }
}
