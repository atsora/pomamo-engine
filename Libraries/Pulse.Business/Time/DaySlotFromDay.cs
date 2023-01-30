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
  public sealed class DaySlotFromDay
    : IRequest<IDaySlot>
  {
    #region Members
    readonly DateTime m_day;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DaySlotFromDay).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="day"></param>
    public DaySlotFromDay (DateTime day)
    {
      m_day = day;
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

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var daySlot = ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedByDay (m_day);
        if (daySlot is null) {
          log.Fatal ($"Get: no day at {m_day}");
          throw new InvalidOperationException ("No day exists at the requested time");
        }
        if (!daySlot.Day.HasValue) {
          log.Fatal ($"Get: day slot found with a null day at {m_day}");
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

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var daySlot = await ModelDAO.ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedByDayAsync (m_day);
        if (daySlot is null) {
          log.Fatal ($"GetAsync: no day at {m_day}");
          throw new InvalidOperationException ("No day exists at the requested time");
        }
        if (!daySlot.Day.HasValue) {
          log.Fatal ($"GetAsync: day slot found with a null day at {m_day}");
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
      return $"Business.Time.DaySlotFromDay.{m_day.DayToIsoString ()}";
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
