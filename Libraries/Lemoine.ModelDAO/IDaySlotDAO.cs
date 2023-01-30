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
  /// DAO interface for IDaySlot.
  /// </summary>
  public interface IDaySlotDAO: IGenericUpdateDAO<IDaySlot, int>
  {
    /// <summary>
    /// Clear the cache
    /// </summary>
    void ClearCache ();

    /// <summary>
    /// Find all the slots with a day but no week number
    /// </summary>
    /// <returns></returns>
    IList<IDaySlot> FindWithNoWeekNumber ();

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IDaySlot> FindOverlapsRange (UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    Task<IList<IDaySlot>> FindOverlapsRangeAsync (UtcDateTimeRange range);

    /// <summary>
    /// Find the day slot at the specified UTC date/time
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    IDaySlot FindAt (Bound<DateTime> at);

    /// <summary>
    /// Find the day slot at the specified UTC date/time
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    Task<IDaySlot> FindAtAsync (Bound<DateTime> at);

    /// <summary>
    /// Find the day slot at the specified UTC date/time
    /// </summary>
    /// <param name="dateTime">in UTC</param>
    /// <param name="useCache">Use the cache if it is available</param>
    /// <returns></returns>
    IDaySlot FindAt (DateTime dateTime, bool useCache);
    
    /// <summary>
    /// Get the day slots which template has not been processed yet in the specified range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IDaySlot> GetNotProcessTemplate (UtcDateTimeRange range);
    
    /// <summary>
    /// Get the day slots which template has not been processed yet in the specified range
    /// </summary>
    /// <param name="range"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    IList<IDaySlot> GetNotProcessTemplate (UtcDateTimeRange range, int limit);
    
    /// <summary>
    /// Find by day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    IDaySlot FindByDay (DateTime day);

    /// <summary>
    /// Find by day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    Task<IDaySlot> FindByDayAsync (DateTime day);

    /// <summary>
    /// Find the processed day slot at the specified date/time
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IDaySlot FindProcessedAt (DateTime dateTime);

    /// <summary>
    /// Find the processed day slot at the specified date/time
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    Task<IDaySlot> FindProcessedAtAsync (DateTime dateTime);

    /// <summary>
    /// Find the processed day slot at the specified day
    /// 
    /// null is returned if the requested day is too old
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    IDaySlot FindProcessedByDay (DateTime day);

    /// <summary>
    /// Find the processed day slot at the specified day asynchronously
    /// 
    /// null is returned if the requested day is too old
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    Task<IDaySlot> FindProcessedByDayAsync (DateTime day);

    /// <summary>
    /// Find the list of day slots in the specified range
    /// 
    /// The range must be included in the range when the day are processed
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IDaySlot> FindProcessedInRange (UtcDateTimeRange range);

    /// <summary>
    /// Find the list of day slots in the specified day range
    /// 
    /// The range must be included in the range when the day are processed
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IDaySlot> FindProcessedInDayRange (DayRange range);

    /// <summary>
    /// Get the system day corresponding to a specified date/time
    /// 
    /// The kind of the specified date/time is taken into account.
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    DateTime GetDay (DateTime dateTime);

    /// <summary>
    /// Get the system day end corresponding to a specified date/time
    /// 
    /// The kind of the specified date/time is taken into account.
    /// 
    /// If the specified date/time is the cut-off time, the previous day is considered.
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    DateTime GetEndDay (DateTime dateTime);

    /// <summary>
    /// Deduce a day range from a date/time range
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    DayRange ConvertToDayRange (UtcDateTimeRange range);

    /// <summary>
    /// Deduce a day range from a date/time range asynchronously
    /// 
    /// This is better to have a read-write transaction, because some data may be computed live.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    Task<DayRange> ConvertToDayRangeAsync (UtcDateTimeRange range);

    /// <summary>
    /// Convert a day to a UTC date/time range
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    UtcDateTimeRange ConvertToUtcDateTimeRange (DateTime day);

    /// <summary>
    /// Convert a day to a UTC date/time range asynchronously
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    Task<UtcDateTimeRange> ConvertToUtcDateTimeRangeAsync (DateTime day);

    /// <summary>
    /// Convert a day range to a UTC date/time range
    /// </summary>
    /// <param name="dayRange"></param>
    /// <returns></returns>
    UtcDateTimeRange ConvertToUtcDateTimeRange (DayRange dayRange);

    /// <summary>
    /// Convert a day range to a UTC date/time range asynchronously
    /// </summary>
    /// <param name="dayRange"></param>
    /// <returns></returns>
    Task<UtcDateTimeRange> ConvertToUtcDateTimeRangeAsync (DayRange dayRange);

    /// <summary>
    /// Get the begin date/time of the corresponding day
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    DateTime GetDayBegin (DateTime day);
    
    /// <summary>
    /// Get the end date/time of the corresponding day
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    DateTime GetDayEnd (DateTime day);

    /// <summary>
    /// Return the date/time range of today
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <returns></returns>
    UtcDateTimeRange GetTodayRange ();
    
    /// <summary>
    /// Process the day slots in the specified range
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range"></param>
    void ProcessInRange (CancellationToken cancellationToken, UtcDateTimeRange range);
  }
}
