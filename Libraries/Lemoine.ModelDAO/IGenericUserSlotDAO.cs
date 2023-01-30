// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for GenericUserSlotDAO
  /// </summary>
  public interface IGenericUserSlotDAO<I>
    where I: ISlot, IPartitionedByUser
  {
    /// <summary>
    /// Find all the slots for the specified user
    /// </summary>
    /// <param name="user">not null</param>
    /// <returns></returns>
    IList<I> FindAll (IUser user);
    
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    I FindAt (IUser user, DateTime dateTime);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<I> FindOverlapsRange (IUser user, UtcDateTimeRange range);    
  }
}
