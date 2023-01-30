// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ShiftMachineAssociation
  /// 
  /// This modification table records any change on a shift
  /// </summary>
  public interface IShiftMachineAssociation: IModification, IPartitionedByMachine
  {
    /// <summary>
    /// Reference to a day
    /// </summary>
    DateTime? Day { get; }
    
    /// <summary>
    /// Reference to a shift
    /// </summary>
    IShift Shift { get; }
    
    /// <summary>
    /// Begin UTC date/time of a shift change
    /// </summary>
    LowerBound<DateTime> Begin { get; }
    
    /// <summary>
    /// End UTC date/time of a shift change
    /// </summary>
    UpperBound<DateTime> End { get; set; }
  }
}
