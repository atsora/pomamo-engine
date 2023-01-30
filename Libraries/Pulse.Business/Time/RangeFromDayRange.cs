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
  /// Request class to get ...
  /// </summary>
  public sealed class RangeFromDayRange
    : IRequest<UtcDateTimeRange>
  {
    readonly DayRange m_dayRange;

    static readonly ILog log = LogManager.GetLogger (typeof (RangeFromDayRange).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayRange"></param>
    public RangeFromDayRange (DayRange dayRange)
    {
      m_dayRange = dayRange;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public UtcDateTimeRange Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      // Optimization using DaySlotFromDay
      try {
        if (m_dayRange.Lower.HasValue && m_dayRange.Upper.HasValue
          && m_dayRange.Lower.Value.Equals (m_dayRange.Upper.Value)) {
          var day = m_dayRange.Lower.Value;
          var daySlot = Lemoine.Business.ServiceProvider
            .Get (new DaySlotFromDay (day));
          if (daySlot is null) {
            log.Error ($"Get: day slot was null, which was unexpected");
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: {daySlot.DateTimeRange} from daySlot at {day}");
            }
            return daySlot.DateTimeRange;
          }
        }
        else { // Not a unique day
          var lower = new LowerBound<DateTime> (null);
          if (m_dayRange.Lower.HasValue) {
            DateTime day;
            if (m_dayRange.LowerInclusive) {
              day = m_dayRange.Lower.Value;
            }
            else {
              day = m_dayRange.Lower.Value.AddDays (1);
            }
            var daySlot = Lemoine.Business.ServiceProvider
              .Get (new DaySlotFromDay (day));
            lower = daySlot.DateTimeRange.Lower;
          }

          var upper = new UpperBound<DateTime> (null);
          if (m_dayRange.Upper.HasValue) {
            DateTime day;
            if (m_dayRange.UpperInclusive) {
              day = m_dayRange.Upper.Value;
            }
            else {
              day = m_dayRange.Upper.Value.AddDays (-1);
            }
            var daySlot = Lemoine.Business.ServiceProvider
              .Get (new DaySlotFromDay (day));
            upper = daySlot.DateTimeRange.Upper;
          }

          if (log.IsDebugEnabled) {
            log.Debug ($"Get: get [{lower},{upper}) from DaySlotFromDay");
          }
          return new UtcDateTimeRange (lower, upper);
        }
      }
      catch (Exception ex) {
        log.Error ($"Get: DaySlotFromDay returned an exception => fallback using ConvertToUtcDateTimeRange", ex);
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .ConvertToUtcDateTimeRange (m_dayRange);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<UtcDateTimeRange> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      // Optimization using DaySlotFromDay
      try {
        if (m_dayRange.Lower.HasValue && m_dayRange.Upper.HasValue
          && m_dayRange.Lower.Value.Equals (m_dayRange.Upper.Value)) {
          var day = m_dayRange.Lower.Value;
          var daySlot = await Lemoine.Business.ServiceProvider
            .GetAsync (new DaySlotFromDay (day));
          if (daySlot is null) {
            log.Error ($"GetAsync: day slot was null, which was unexpected");
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetAsync: {daySlot.DateTimeRange} from daySlot at {day}");
            }
            return daySlot.DateTimeRange;
          }
        }
        else { // Not a unique day
          var lower = new LowerBound<DateTime> (null);
          if (m_dayRange.Lower.HasValue) {
            DateTime day;
            if (m_dayRange.LowerInclusive) {
              day = m_dayRange.Lower.Value;
            }
            else {
              day = m_dayRange.Lower.Value.AddDays (1);
            }
            var daySlot = await Lemoine.Business.ServiceProvider
              .GetAsync (new DaySlotFromDay (day));
            lower = daySlot.DateTimeRange.Lower;
          }

          var upper = new UpperBound<DateTime> (null);
          if (m_dayRange.Upper.HasValue) {
            DateTime day;
            if (m_dayRange.UpperInclusive) {
              day = m_dayRange.Upper.Value;
            }
            else {
              day = m_dayRange.Upper.Value.AddDays (-1);
            }
            var daySlot = await Lemoine.Business.ServiceProvider
              .GetAsync (new DaySlotFromDay (day));
            upper = daySlot.DateTimeRange.Upper;
          }

          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: get [{lower},{upper}) from DaySlotFromDay");
          }
          return new UtcDateTimeRange (lower, upper);
        }
      }
      catch (Exception ex) {
        log.Error ($"GetAsync: DaySlotFromDay returned an exception => fallback using ConvertToUtcDateTimeRange", ex);
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return await ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .ConvertToUtcDateTimeRangeAsync (m_dayRange);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Time.RangeFromDayRange.{m_dayRange.ToString (d => d.DayToIsoString ())}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (UtcDateTimeRange data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<UtcDateTimeRange> data)
    {
      if (null == data.Value) {
        return true;
      }
      else {
        return true;
      }
    }
    #endregion // IRequest implementation
  }
}
