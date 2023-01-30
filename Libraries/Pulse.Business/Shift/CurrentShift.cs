// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.Shift
{
  /// <summary>
  /// Request class to get the current shift
  /// </summary>
  public sealed class CurrentShift
    : IRequest<IShiftSlot>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CurrentShift).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CurrentShift ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public IShiftSlot Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return ModelDAO.ModelDAOHelper.DAOFactory.ShiftSlotDAO
          .FindAt (DateTime.UtcNow);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IShiftSlot> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return await ModelDAO.ModelDAOHelper.DAOFactory.ShiftSlotDAO
          .FindAtAsync (DateTime.UtcNow);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.CurrentShift";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IShiftSlot data)
    {
      var configTimeOut = CacheTimeOut.Config.GetTimeSpan ();
      if (data is null) {
        return configTimeOut;
      }
      else if (!data.EndDateTime.HasValue) {
        return configTimeOut;
      }
      else {
        var utcNow = DateTime.UtcNow;
        if (data.EndDateTime.Value <= utcNow) {
          return TimeSpan.FromTicks (0);
        }
        else {
          var endIn = data.EndDateTime.Value.Subtract (utcNow);
          return endIn < configTimeOut
            ? endIn
            : configTimeOut;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IShiftSlot> data)
    {
      if (data.Value is null) {
        return false;
      }
      else {
        return Bound.Compare<DateTime> (data.Value.EndDateTime, DateTime.UtcNow) <= 0;
      }
    }
    #endregion // IRequest implementation
  }
}
