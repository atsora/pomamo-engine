// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table UserAttendance
  /// 
  /// This new modification table records
  /// when a user checks in or checks out in the site.
  /// It can be associated to a time clock system.
  /// </summary>
  public interface IUserAttendance: IGlobalModification
  {
    /// <summary>
    /// Reference to a User
    /// 
    /// Not null
    /// </summary>
    IUser User { get; }
    
    /// <summary>
    /// (optional) UTC date/time of a user check-in
    /// </summary>
    Nullable<DateTime> Begin { get; set; }
    
    /// <summary>
    /// (optional) UTC date/time of a user check-out
    /// </summary>
    Nullable<DateTime> End { get; set; }
    
    /// <summary>
    /// (optional) Associated shift
    /// </summary>
    IShift Shift { get; set; }
  }
}
