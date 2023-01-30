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
  public sealed class DayRangeFromRange
    : IRequest<DayRange>
  {
    #region Members
    readonly UtcDateTimeRange m_range;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DayRangeFromRange).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="range"></param>
    public DayRangeFromRange (UtcDateTimeRange range)
    {
      m_range = range;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public DayRange Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      // Optimization
      try {
        var lowerDay = new LowerBound<DateTime> (null);
        if (m_range.Lower.HasValue) {
          DateTime at;
          if (m_range.LowerInclusive) {
            at = m_range.Lower.Value;
          }
          else {
            at = m_range.Lower.Value.AddTicks (1);
          }
          var daySlot = Lemoine.Business.ServiceProvider
            .Get (new DayAt (at));
          lowerDay = daySlot.Day.Value;
        }

        var upperDay = new UpperBound<DateTime> (null);
        if (m_range.Upper.HasValue) {
          DateTime at;
          if (m_range.UpperInclusive) {
            at = m_range.Upper.Value;
          }
          else {
            at = m_range.Upper.Value.AddTicks (-1);
          }
          var daySlot = Lemoine.Business.ServiceProvider
            .Get (new DayAt (at));
          upperDay = daySlot.Day.Value;
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"Get: [{lowerDay},{upperDay}] from optimization");
        }
        return new DayRange (lowerDay, upperDay, true, true);
      }
      catch (Exception ex) {
        log.Warn ($"Get: exception in optimization", ex);
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .ConvertToDayRange (m_range);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<DayRange> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      // Optimization
      try {
        var lowerDay = new LowerBound<DateTime> (null);
        if (m_range.Lower.HasValue) {
          DateTime at;
          if (m_range.LowerInclusive) {
            at = m_range.Lower.Value;
          }
          else {
            at = m_range.Lower.Value.AddTicks (1);
          }
          var daySlot = await Lemoine.Business.ServiceProvider
            .GetAsync (new DayAt (at));
          lowerDay = daySlot.Day.Value;
        }

        var upperDay = new UpperBound<DateTime> (null);
        if (m_range.Upper.HasValue) {
          DateTime at;
          if (m_range.UpperInclusive) {
            at = m_range.Upper.Value;
          }
          else {
            at = m_range.Upper.Value.AddTicks (-1);
          }
          var daySlot = await Lemoine.Business.ServiceProvider
            .GetAsync (new DayAt (at));
          upperDay = daySlot.Day.Value;
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"GetAsync: [{lowerDay},{upperDay}] from optimization");
        }
        return new DayRange (lowerDay, upperDay, true, true);
      }
      catch (Exception ex) {
        log.Warn ($"GetAsync: exception in optimization", ex);
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return await ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .ConvertToDayRangeAsync (m_range);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Time.DayRangeFromRange.{m_range.ToString (s => s.ToIsoString ())}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (DayRange data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<DayRange> data)
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
