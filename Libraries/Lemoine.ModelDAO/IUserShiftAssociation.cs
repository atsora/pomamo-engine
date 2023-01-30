// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table UserShiftAssociation
  /// 
  /// This new modification table records
  /// any new association between a user and a shift
  /// </summary>
  public interface IUserShiftAssociation: IGlobalModification, IPeriodAssociation
  {
    /// <summary>
    /// Reference to a User
    /// 
    /// Not null
    /// </summary>
    IUser User { get; }
    
    /// <summary>
    /// (optional) Associated shift
    /// </summary>
    IShift Shift { get; set; }
  }
}
