// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.ExceptionManagement;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Extensions.Web.Services
{
  /// <summary>
  /// Base case for cached service which has only a "current" cache timeout.
  /// Cache based on URL of request.
  /// 
  /// The GET implementation is asynchronous
  /// </summary>
  public abstract class GenericAsyncCachedService<InputDTO>
    : ICachedHandler, ISyncCachedService<InputDTO>
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericSyncCachedService<InputDTO>).FullName);

    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Default cache time/out
    /// </summary>
    public CacheTimeOut DefaultCacheTimeOut { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultCacheTimeOut"></param>
    protected GenericAsyncCachedService (Lemoine.Core.Cache.CacheTimeOut defaultCacheTimeOut)
    {
      this.DefaultCacheTimeOut = defaultCacheTimeOut;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get without cache for NServiceKit
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object GetWithoutCache (InputDTO request)
    {
      return System.Threading.Tasks.Task
        .Run (async () => await Get (request)).Result;
    }

    /// <summary>
    /// Get implementation to write (asynchronous)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public abstract Task<object> Get (InputDTO request);

    /// <summary>
    /// Build the cache key from the URL
    /// 
    /// Default is the URL itself
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    protected virtual string GetCacheKey (string url)
    {
      return url;
    }

    /// <summary>
    /// <see cref="ICachedHandler"/>
    /// </summary>
    /// <param name="pathQuery"></param>
    /// <param name="requestDTO"></param>
    /// <param name="outputDTO">not considered here</param>
    /// <returns></returns>
    public virtual TimeSpan GetCacheTimeOut (string pathQuery, object requestDTO, object outputDTO)
    {
      return GetCacheTimeOut (pathQuery, (InputDTO)requestDTO);
    }

    /// <summary>
    /// Get the cache time out
    /// 
    /// Default is taken from CurrentCacheTimeOut
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    public virtual TimeSpan GetCacheTimeOut (string url, InputDTO requestDTO)
    {
      return this.DefaultCacheTimeOut.GetTimeSpan ();
    }
    #endregion // Methods
  }
}
