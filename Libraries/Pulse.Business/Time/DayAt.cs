// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business;
using Lemoine.Conversion;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.Time
{
  /// <summary>
  /// Request class to get the day at a specific time
  /// </summary>
  public sealed class DayAt
    : IRequest<IDaySlot>
  {
    #region Members
    readonly DateTime m_at;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DayAt).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="at"></param>
    public DayAt (DateTime at)
    {
      m_at = at;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public IDaySlot Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      // Optimization using the DaySlotFromDay service
      try {
        if (m_at.Kind.Equals (DateTimeKind.Unspecified)) {
          log.Error ($"Get: unspecified date/time kind for {m_at}");
        }
        DateTime localDateTime = m_at.ToLocalTime ();
        DateTime utcDateTime = m_at.ToUniversalTime ();
        DateTime day;
        IDaySlot daySlot;

        // 1. Try first localDateTime.Date
        day = localDateTime.Date;
        daySlot = Lemoine.Business.ServiceProvider
          .Get (new DaySlotFromDay (day));
        if (daySlot?.DateTimeRange?.ContainsElement (utcDateTime) ?? false) {
          Debug.Assert (day.Equals (daySlot.Day.Value));
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: data from local date {day} {daySlot?.DateTimeRange} is ok");
          }
          return daySlot;
        }

        // 2. Try next the previous day or the next day
        if (localDateTime.TimeOfDay < TimeSpan.FromHours (12)) { // Try the previous day
          day = day.AddDays (-1).Date;
        }
        else { // Try the next day
          day = day.AddDays (+1).Date;
        }
        daySlot = Lemoine.Business.ServiceProvider
          .Get (new DaySlotFromDay (day));
        if (daySlot?.DateTimeRange?.ContainsElement (utcDateTime) ?? false) {
          Debug.Assert (day.Equals (daySlot.Day.Value));
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: data from other date {day} {daySlot?.DateTimeRange} is ok");
          }
          return daySlot;
        }
      }
      catch (Exception ex) {
        log.Warn ($"Get: optimization with DaySlotFromDay ended with exception", ex);
      }

      if (log.IsWarnEnabled) {
        log.Warn ($"Get: day at {m_at} could not be retrieved with DaySlotFromDay => use FindProcessedAt instead");
      }
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var daySlot = ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedAt (m_at);
        if (daySlot is null) {
          log.Fatal ($"Get: no day at {m_at}");
          throw new InvalidOperationException ("No day exists at the requested time");
        }
        if (!daySlot.Day.HasValue) {
          log.Fatal ($"Get: day slot found with a null day at {m_at}");
        }
        return daySlot;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IDaySlot> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      // Optimization using the DaySlotFromDay service
      try {
        if (m_at.Kind.Equals (DateTimeKind.Unspecified)) {
          log.Error ($"GetAsync: unspecified date/time kind for {m_at}");
        }
        DateTime localDateTime = m_at.ToLocalTime ();
        DateTime utcDateTime = m_at.ToUniversalTime ();
        DateTime day;
        IDaySlot daySlot;

        // 1. Try first localDateTime.Date
        day = localDateTime.Date;
        daySlot = await Lemoine.Business.ServiceProvider
          .GetAsync (new DaySlotFromDay (day));
        if (daySlot?.DateTimeRange?.ContainsElement (utcDateTime) ?? false) {
          Debug.Assert (day.Equals (daySlot.Day.Value));
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: data from local date {day} {daySlot?.DateTimeRange} is ok");
          }
          return daySlot;
        }

        // 2. Try next the previous day or the next day
        if (localDateTime.TimeOfDay < TimeSpan.FromHours (12)) { // Try the previous day
          day = day.AddDays (-1).Date;
        }
        else { // Try the next day
          day = day.AddDays (+1).Date;
        }
        daySlot = await Lemoine.Business.ServiceProvider
          .GetAsync (new DaySlotFromDay (day));
        if (daySlot?.DateTimeRange?.ContainsElement (utcDateTime) ?? false) {
          Debug.Assert (day.Equals (daySlot.Day.Value));
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: data from other date {day} {daySlot?.DateTimeRange} is ok");
          }
          return daySlot;
        }
      }
      catch (Exception ex) {
        log.Warn ($"GetAsync: optimization with DaySlotFromDay ended with exception", ex);
      }

      if (log.IsWarnEnabled) {
        log.Warn ($"GetAsync: day at {m_at} could not be retrieved with DaySlotFromDay => use FindProcessedAt instead");
      }
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var daySlot = await ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedAtAsync (m_at);
        if (daySlot is null) {
          log.Fatal ($"GetAsync: no day at {m_at}");
          throw new InvalidOperationException ("No day exists at the requested time");
        }
        if (!daySlot.Day.HasValue) {
          log.Fatal ($"GetAsync: day slot found with a null day at {m_at}");
        }
        return daySlot;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Time.DayAt.{m_at.ToIsoString ()}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IDaySlot data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IDaySlot> data)
    {
      if (data.Value is null) {
        return false;
      }
      else {
        return true;
      }
    }
    #endregion // IRequest implementation
  }
}
