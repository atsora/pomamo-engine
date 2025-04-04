// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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

namespace Lemoine.Web
{
  /// <summary>
  /// Base case for cached service which has only a "current" cache timeout.
  /// Cache based on URL of request.
  /// </summary>
  public abstract class GenericCachedService<InputDTO> : ICachedHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericCachedService<InputDTO>).FullName);

    CacheTimeOut DefaultCacheTimeOut { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultCacheTimeOut"></param>
    protected GenericCachedService (Lemoine.Core.Cache.CacheTimeOut defaultCacheTimeOut)
    {
      this.DefaultCacheTimeOut = defaultCacheTimeOut;
    }

    /// <summary>
    /// Get without cache
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual object GetWithoutCache (InputDTO request)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Get implementation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual Task<object> Get (InputDTO request)
    {
      var result = GetWithoutCache (request);
      return Task.FromResult (result);
    }

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
      if (requestDTO is InputDTO) {
        return GetCacheTimeOut (pathQuery, (InputDTO)requestDTO, outputDTO);
      }
      else {
        log.Fatal ($"GetCacheTimeOut: {requestDTO} is not of type {typeof (InputDTO)}");
        Debug.Assert (false);
        return CacheTimeOut.NoCache.GetTimeSpan ();
      }
    }

    /// <summary>
    /// Get the cache time out (Asp service only)
    /// 
    /// Override it by the implementation if needed
    /// 
    /// By default, call GetCacheTimeout without outputDTO
    /// </summary>
    /// <param name="pathQuery"></param>
    /// <param name="requestDTO"></param>
    /// <param name="outputDTO">not considered here</param>
    /// <returns></returns>
    protected virtual TimeSpan GetCacheTimeOut (string pathQuery, InputDTO requestDTO, object outputDTO)
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
    protected virtual TimeSpan GetCacheTimeOut (string url, InputDTO requestDTO)
    {
      return this.DefaultCacheTimeOut.GetTimeSpan ();
    }

  }
}
