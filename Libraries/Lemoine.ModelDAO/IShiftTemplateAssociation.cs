// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ShiftTemplateAssociation
  /// 
  /// This new table is designed to add a shift Template.
  /// </summary>
  public interface IShiftTemplateAssociation: IGlobalModification
  {
    /// <summary>
    /// Reference to the shift Template
    /// 
    /// Not null
    /// </summary>
    IShiftTemplate ShiftTemplate { get; }
    
    /// <summary>
    /// UTC begin date/time of the association
    /// </summary>
    LowerBound<DateTime> Begin { get; set; }

    /// <summary>
    /// UTC end date/time (optional) of the association
    /// </summary>
    UpperBound<DateTime> End { get; set; }

    /// <summary>
    /// Force re-building the shift templates
    /// 
    /// Default is False
    /// </summary>
    bool Force { get; set; }
  }
}
