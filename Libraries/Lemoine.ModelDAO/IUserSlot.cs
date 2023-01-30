// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table UserSlot
  /// 
  /// Analysis table where are stored all the user periods on the site.
  /// </summary>
  public interface IUserSlot: ISlot, IPartitionedByUser, IComparable<IUserSlot>, ICloneable
  {
    /// <summary>
    /// (optional) Associated shift
    /// </summary>
    IShift Shift { get; set; }
  }
}
