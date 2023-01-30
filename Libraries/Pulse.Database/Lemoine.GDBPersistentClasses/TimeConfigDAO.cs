// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.GDBMigration;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ITimeConfigDAO">ITimeConfigDAO</see>
  /// </summary>
  public class TimeConfigDAO
    : ITimeConfigDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TimeConfigDAO).FullName);

    /// <summary>
    /// Get the cut-off time in configuration for today
    /// </summary>
    /// <returns></returns>
    [Obsolete("Try to avoid using the cut-off time directly any more, use DaySlot objects instead")]
    public TimeSpan GetCutOffTime ()
    {
      TimeSpan beginTimeSpan = GetTodayBeginUtcDateTime ().ToLocalTime ().TimeOfDay;
      if (TimeSpan.FromHours (12) < beginTimeSpan) {
        return beginTimeSpan.Subtract (TimeSpan.FromHours (24));
      }
      else {
        return beginTimeSpan;
      }
    }
    
    /// <summary>
    /// Get the system day corresponding to a specified date/time
    /// 
    /// The kind of the specified date/time is taken into account.
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public DateTime GetDay (DateTime dateTime)
    {
      return ModelDAOHelper.DAOFactory.DaySlotDAO.GetDay (dateTime);
    }
    
    /// <summary>
    /// Get the system end day corresponding to a specified date/time
    /// 
    /// The kind of the specified date/time is taken into account.
    /// 
    /// If the specified date/time is the cut-off time, the previous day is considered.
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public DateTime GetEndDay (DateTime dateTime)
    {
      return ModelDAOHelper.DAOFactory.DaySlotDAO.GetEndDay (dateTime);
    }
    
    /// <summary>
    /// Get the system day (like in database) corresponding to now
    /// taking into account the cut off time.
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <returns></returns>
    public DateTime GetToday ()
    {
      return GetDay (DateTime.UtcNow);
    }
    
    /// <summary>
    /// Get the begin local date/time of a system day
    /// taking into account the cut off time
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day">UTC date/time</param>
    /// <returns></returns>
    DateTime GetDayBeginLocalDateTime (DateTime day)
    {
      Debug.Assert (DateTimeKind.Utc != day.Kind);
     
      return GetDayBeginUtcDateTime (day).ToLocalTime ();
    }

    /// <summary>
    /// Get the begin UTC date/time of a system day
    /// taking into account the cut off time
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day">UTC date/time</param>
    /// <returns></returns>
    public DateTime GetDayBeginUtcDateTime (DateTime day)
    {
      return ModelDAOHelper.DAOFactory.DaySlotDAO.GetDayBegin (day);
    }
    
    /// <summary>
    /// Get the end local date/time of a system day
    /// taking into account the cut off time
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    DateTime GetDayEndLocalDateTime (DateTime day)
    {
      Debug.Assert (DateTimeKind.Utc != day.Kind);

      return GetDayEndUtcDateTime (day).ToLocalTime ();
    }

    /// <summary>
    /// Get the end local date/time of a system day
    /// taking into account the cut off time
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public DateTime GetDayEndUtcDateTime (DateTime day)
    {
      return ModelDAOHelper.DAOFactory.DaySlotDAO.GetDayEnd (day);
    }

    /// <summary>
    /// Get the begin UTC date/time of today
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <returns></returns>
    public DateTime GetTodayBeginUtcDateTime ()
    {
      IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
      Debug.Assert (null != daySlot);
      Debug.Assert (daySlot.Day.HasValue);
      Debug.Assert (daySlot.BeginDateTime.HasValue);
      return daySlot.BeginDateTime.Value;
    }

    /// <summary>
    /// Get the end UTC date/time of today
    /// 
    /// The transaction must be in read-write state, because some data may be computed live.
    /// </summary>
    /// <returns></returns>
    public DateTime GetTodayEndUtcDateTime ()
    {
      IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
      Debug.Assert (null != daySlot);
      Debug.Assert (daySlot.Day.HasValue);
      Debug.Assert (daySlot.EndDateTime.HasValue);
      return daySlot.EndDateTime.Value;
    }
  }
}
