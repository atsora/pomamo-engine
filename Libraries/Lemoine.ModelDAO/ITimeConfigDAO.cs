// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface to get different time configuration and utility methods
  /// 
  /// Use DaySlotDAO instead when it is more appropriate
  /// </summary>
  public interface ITimeConfigDAO
  {
    /// <summary>
    /// Get the cut-off time in configuration
    /// </summary>
    /// <returns></returns>
    [Obsolete("Try to avoid using the cut-off time directly any more, use DaySlot objects instead")]
    TimeSpan GetCutOffTime ();
    
    /// <summary>
    /// Get the system day (like in database) corresponding to a specified UTC date/time
    /// taking into account the cut off time.
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    DateTime GetDay (DateTime utcDateTime);
    
    /// <summary>
    /// Get the system day (like in database) corresponding to a specified UTC end date/time
    /// considering the cut-off time.
    /// 
    /// If the specified date/time is the cut-off time, the previous day is considered.
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    DateTime GetEndDay (DateTime utcDateTime);
    
    /// <summary>
    /// Get the System day (like in database) corresponding to now
    /// taking into account the cut off time.
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <returns></returns>
    DateTime GetToday ();

    /// <summary>
    /// Get the begin UTC date/time of a system day
    /// taking into account the cut off time
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    DateTime GetDayBeginUtcDateTime (DateTime day);

    /// <summary>
    /// Get the end UTC date/time of a system day
    /// taking into account the cut off time
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    DateTime GetDayEndUtcDateTime (DateTime day);
    
    /// <summary>
    /// Get the begin UTC date/time of today
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <returns></returns>
    DateTime GetTodayBeginUtcDateTime ();

    /// <summary>
    /// Get the end UTC date/time of today
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <returns></returns>
    DateTime GetTodayEndUtcDateTime ();
  }
}
