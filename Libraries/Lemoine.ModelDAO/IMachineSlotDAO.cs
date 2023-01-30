// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineSlot.
  /// </summary>
  public interface IMachineSlotDAO<I>
    where I: ISlot, IPartitionedByMachine
  {
    /// <summary>
    /// Find all the slots for the specified machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IList<I> FindAll (IMachine machine);

    /// <summary>
    /// Find all the slots for the specified machine asynchronously
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<I>> FindAllAsync (IMachine machine);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    I FindAt (IMachine machine, Bound<DateTime> dateTime);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time asynchronously
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<I> FindAtAsync (IMachine machine, Bound<DateTime> dateTime);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<I> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range asynchronously
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<I>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified day range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<I> FindInDayRange (IMachine machine, DayRange range);

    /// <summary>
    /// Find all the slots that overlap the specified day range asynchronously
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<I>> FindInDayRangeAsync (IMachine machine, DayRange range);

    /// <summary>
    /// Find the slots at the specified day
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    IList<I> FindAtDay (IMachine machine, DateTime day);

    /// <summary>
    /// Find the slots at the specified day asynchronously
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<I>> FindAtDayAsync (IMachine machine, DateTime day);

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    IEnumerable<I> FindFirstOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending);

    /// <summary>
    /// Find the first n elements in the range in the specified order asynchronously
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IEnumerable<I>> FindFirstOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range, int n, bool descending);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<I> FindOverlapsRangeAscending (IMachine machine,
                                               UtcDateTimeRange range,
                                               TimeSpan step);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a descending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<I> FindOverlapsRangeDescending (IMachine machine,
                                                UtcDateTimeRange range,
                                                TimeSpan step);
  }
}
