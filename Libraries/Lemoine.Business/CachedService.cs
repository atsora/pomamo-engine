// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;

namespace Lemoine.Business
{
  /// <summary>
  /// Class to store nullable values in cache
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class CacheValue<T>
  {
    /// <summary>
    /// Request
    /// </summary>
    public IRequest<T> Request { get; private set; }

    /// <summary>
    /// Accessor
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="request"></param>
    /// <param name="v"></param>
    public CacheValue (IRequest<T> request, T v)
    {
      this.Request = request;
      this.Value = v;
    }
  }

  /// <summary>
  /// Service using any ICacheClient
  /// </summary>
  public sealed class CachedService : IService
  {
    static readonly TimeSpan MAX_CACHE_TIMEOUT = CacheTimeOut.Permanent.GetTimeSpan ();
    static readonly string CULTURE_SUFFIX = ".culture=";

    readonly ICacheClient m_cacheClient;

    static readonly ILog log = LogManager.GetLogger (typeof (CachedService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cacheClient">not null</param>
    public CachedService (ICacheClient cacheClient)
    {
      Debug.Assert (null != cacheClient);

      m_cacheClient = cacheClient;
    }

    /// <summary>
    /// Get the result of a request
    /// </summary>
    /// <param name="request">not null</param>
    /// <returns></returns>
    public T Get<T> (IRequest<T> request)
    {
      Debug.Assert (null != request);

      string key = GetCacheKey (request);
      CacheValue<T> cached = m_cacheClient.Get<CacheValue<T>> (key);
      if (null != cached) {
        if (request.IsCacheValid (cached)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: key {key} in cache and valid: {cached.Value}");
          }
          return cached.Value;
        }
      }

      // null == cached or cache not valid
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: key {key} not in cache");
      }
      T data = request.Get ();
      try {
        SetCache (request, data, key);
      }
      catch (Exception ex) {
        log.Error ($"Get: SetCache returned an exception for key {key} => skip the cache", ex);
      }
      return data;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<T> GetAsync<T> (IRequest<T> request)
    {
      Debug.Assert (null != request);

      string key = GetCacheKey (request);
      CacheValue<T> cached = m_cacheClient.Get<CacheValue<T>> (key);
      if (null != cached) {
        if (request.IsCacheValid (cached)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: key {key} in cache and valid: {cached.Value}");
          }
          return cached.Value;
        }
      }

      // null == cached or cache not valid
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: key {key} not in cache");
      }
      T data = await request.GetAsync ();
      try {
        SetCache (request, data, key);
      }
      catch (Exception ex) {
        log.Error ($"Get: SetCache returned an exception for key {key} => skip the cache", ex);
      }
      return data;
    }

    void SetCache<T> (IRequest<T> request, T data, string key)
    {
      try {
        var cacheTimeout = request.GetCacheTimeout (data);
        if (log.IsDebugEnabled) {
          log.Debug ($"SetCache: cache timeout is {cacheTimeout} for key {key}");
        }
        if (MAX_CACHE_TIMEOUT < cacheTimeout) {
          log.Error ($"SetCache: cache timeout {cacheTimeout} was greated that the maximum {MAX_CACHE_TIMEOUT} for key {key} => use the maximum");
          cacheTimeout = MAX_CACHE_TIMEOUT;
        }
        DateTime expiresAt = DateTime.UtcNow.Add (cacheTimeout);
        if (false == m_cacheClient.Set (key, new CacheValue<T> (request, data), expiresAt)) {
          log.Error ($"SetCache: cache data key={key} could not be set");
        }
      }
      catch (Exception ex) {
        log.Error ($"SetCache: exception while setting the cache for key {key}", ex);
      }
    }

    /// <summary>
    /// Get the data in cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns>null if no cache data</returns>
    public CacheValue<T> GetCacheData<T> (IRequest<T> request)
    {
      Debug.Assert (null != request);

      string key = GetCacheKey (request);
      CacheValue<T> cached = m_cacheClient.Get<CacheValue<T>> (key);
      return cached;
    }

    /// <summary>
    /// Cache key of a request: the key of the request, followed by the UI culture
    /// (for example Business.Operation.PartProductionRange.12.culture=fr),
    /// because the result may contain translated strings and the UI culture may change
    /// from a web request to another one (see RequestCultureMiddleware).
    ///
    /// The culture is added at the end as a last dot-separated part, so that the cache invalidation regexes,
    /// that are based on the prefix of the key and on its parts, still match.
    /// </summary>
    internal static string GetCacheKey<T> (IRequest<T> request) =>
      GetCacheKey (request.GetCacheKey (), CultureInfo.CurrentUICulture);

    internal static string GetCacheKey (string requestCacheKey, CultureInfo cultureInfo) =>
      $"{requestCacheKey}{CULTURE_SUFFIX}{cultureInfo.Name}";

    /// <summary>
    /// Remove from the cache the result of a request, whatever the UI culture it was computed with
    ///
    /// Use it instead of <see cref="ICacheClient.Remove(string)"/> with the key of the request,
    /// which does not match any more the key in cache because of the UI culture suffix (.culture=fr)
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="requestCacheKey">key returned by <see cref="IRequest{T}.GetCacheKey"/></param>
    public static void RemoveRequest (ICacheClient cacheClient, string requestCacheKey)
    {
      if (cacheClient is null) {
        log.Fatal ("RemoveRequest: cacheClient is null");
        throw new ArgumentNullException (nameof (cacheClient));
      }
      if (requestCacheKey is null) {
        log.Fatal ("RemoveRequest: requestCacheKey is null");
        throw new ArgumentNullException (nameof (requestCacheKey));
      }

      cacheClient.RemoveByRegex ($"^{Regex.Escape (requestCacheKey + CULTURE_SUFFIX)}");
    }

    // Note: using IsDefault() is not necessary any more because the cache contains a CacheValue is just a normal object (nullable)
  }
}
