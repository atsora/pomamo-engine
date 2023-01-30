// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table UserShiftSlot
  /// 
  /// Analysis table where are stored all the periods where a user is associated to a shift.
  /// </summary>
  public interface IUserShiftSlot: ISlot, IPartitionedByUser, IComparable<IUserShiftSlot>, ICloneable
  {
    /// <summary>
    /// Associated shift (not null)
    /// </summary>
    IShift Shift { get; set; }
  }
}
