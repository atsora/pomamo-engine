// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.WebMiddleware.Cache
{
  /// <summary>
  /// Cache action
  /// </summary>
  [Flags]
  public enum CacheAction
  {
    /// <summary>
    /// No action is defined
    /// </summary>
    None = 0,
    /// <summary>
    /// Use the cache (default)
    /// </summary>
    Default = 1,
    /// <summary>
    /// Use the cache
    /// </summary>
    UseCache = 1,
    /// <summary>
    /// Do not use the cache
    /// </summary>
    NoCache = 2,
    /// <summary>
    /// Invalid the cache
    /// </summary>
    InvalidCache = 4,
    /// <summary>
    /// Cache action only (do not run the request)
    /// </summary>
    ActionOnly = 8,
    /// <summary>
    /// Request with no cache
    /// </summary>
    RequestWithNoCache = CacheAction.NoCache | CacheAction.InvalidCache,
    /// <summary>
    /// Clear only the cache
    /// </summary>
    Clear = CacheAction.InvalidCache | CacheAction.ActionOnly,
  }

  public static class CacheActionExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (CacheActionExtensions).FullName);

    public static CacheAction ExtractCacheAction (this Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
      return httpContext.Request.Query.ExtractCacheAction ();
    }

    public static CacheAction ExtractCacheAction (this IQueryCollection queryCollection)
    {
      KeyValuePair<string, StringValues> query;
      try {
        query = queryCollection.First (q => q.Key.Equals ("cache", StringComparison.InvariantCultureIgnoreCase));
      }
      catch (Exception) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ExtactCacheAction: no cache action found => use default");
        }
        return CacheAction.Default;
      }
      var cacheAction = CacheAction.None;
      if (query.Value.Any (s => s.Equals ("no", StringComparison.InvariantCultureIgnoreCase))) {
        cacheAction |= CacheAction.RequestWithNoCache;
      }
      if (query.Value.Any (s => s.Equals ("clear", StringComparison.InvariantCultureIgnoreCase))) {
        cacheAction |= CacheAction.Clear;
      }
      if (cacheAction.Equals (CacheAction.None)) {
        if (log.IsWarnEnabled) {
          log.Warn ($"ExtractCacheAction: invalid cache query {query.ToString ()} => return the default one");
        }
        return CacheAction.Default;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"ExtractCacheAction: cache action is {cacheAction}");
      }
      return cacheAction;
    }
  }
}
