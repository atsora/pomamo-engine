// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ISlot.
  /// </summary>
  public interface IGlobalSlotDAO<I>
    where I: ISlot
  {
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<I> FindOverlapsRange (UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range asynchronously
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<I>> FindOverlapsRangeAsync (UtcDateTimeRange range);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    I FindAt (DateTime dateTime);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time asynchronously
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<I> FindAtAsync (DateTime dateTime);
  }
}
