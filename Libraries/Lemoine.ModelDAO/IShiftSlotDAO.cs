// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IShiftSlot.
  /// </summary>
  public interface IShiftSlotDAO: IGenericUpdateDAO<IShiftSlot, int>
  {
    /// <summary>
    /// Find the shift slots for a specific day and shift
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="day"></param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    IList<IShiftSlot> FindWith (DateTime day, IShift shift);

    /// <summary>
    /// Find the shift slots for a specific day and shift
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="day"></param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    Task<IList<IShiftSlot>> FindWithAsync (DateTime day, IShift shift);

    /// <summary>
    /// Find all the shift slots that overlap the specific range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IShiftSlot> FindOverlapsRange (UtcDateTimeRange range);
    
    /// <summary>
    /// Find the shift slot at the specified UTC date/time
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    IShiftSlot FindAt (Bound<DateTime> at);

    /// <summary>
    /// Find the shift slot at the specified UTC date/time asynchronously
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    Task<IShiftSlot> FindAtAsync (Bound<DateTime> at);

    /// <summary>
    /// Find all the shift slots at the specified day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    IList<IShiftSlot> FindAtDay (DateTime day);
    
    /// <summary>
    /// Find all the shift slots at the specified day
    /// with an eager fetch of the breaks
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    IList<IShiftSlot> FindAtDayWithBreaks (DateTime day);
    
    /// <summary>
    /// Return true if the shift is defined
    /// </summary>
    /// <param name="shift">not null</param>
    /// <param name="day"></param>
    /// <returns></returns>
    bool IsDefined (IShift shift, DateTime day);

    /// <summary>
    /// Get the shift slots which template has not been processed yet in the specified range in ascending order
    /// </summary>
    /// <param name="range"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    IEnumerable<IShiftSlot> GetNotProcessTemplate (UtcDateTimeRange range, int limit);
    
    /// <summary>
    /// Get the shift slots which template has not been processed yet in the specified range in ascending order
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IShiftSlot> GetNotProcessTemplate (UtcDateTimeRange range);
    
    /// <summary>
    /// Process the shift slots in the specified range
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range"></param>
    void ProcessInRange (CancellationToken cancellationToken, UtcDateTimeRange range);

    /// <summary>
    /// Return the first slot whose beginning is before a specific date
    /// The end can be after the specific date
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    IShiftSlot GetFirstSlotBeginningBefore (DateTime utcDateTime);

    /// <summary>
    /// Return the first slot whose end is after (or equal to) a specific date
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    IShiftSlot GetFirstSlotEndAfter (DateTime utcDateTime);
  }
}
