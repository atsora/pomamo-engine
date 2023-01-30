// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
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

    #region Members
    readonly ICacheClient m_cacheClient;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CachedService).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cacheClient">not null</param>
    public CachedService (ICacheClient cacheClient)
    {
      Debug.Assert (null != cacheClient);

      m_cacheClient = cacheClient;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the result of a request
    /// </summary>
    /// <param name="request">not null</param>
    /// <returns></returns>
    public T Get<T> (IRequest<T> request)
    {
      Debug.Assert (null != request);

      string key = request.GetCacheKey ();
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

      string key = request.GetCacheKey ();
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

      string key = request.GetCacheKey ();
      CacheValue<T> cached = m_cacheClient.Get<CacheValue<T>> (key);
      return cached;
    }

    // Note: using IsDefault() is not necessary any more because the cache contains a CacheValue is just a normal object (nullable)
    #endregion // Methods
  }
}
