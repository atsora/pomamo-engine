// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.ModelDAO;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.Business.Computer
{
  /// <summary>
  /// Business class to know if the current computer is lctr
  /// </summary>
  public sealed class IsLctr
    : IRequest<bool>
  {
    static readonly string LCTR_FORCE_KEY = "lctr.force";
    static readonly bool LCTR_FORCE_DEFAULT = false;
    
    static readonly ILog log = LogManager.GetLogger(typeof (IsLctr).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public IsLctr ()
    {
    }
    #endregion // Constructors

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>true or false</returns>
    public bool Get()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (LCTR_FORCE_KEY,
                                                   LCTR_FORCE_DEFAULT)) {
        return true;
      }

      return ComputerTest.IsLctr ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey()
    {
      return "Business.Computer.IsLctr";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<bool> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (bool data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
