// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Conversion;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.Business.DynamicTimes
{
  /// <summary>
  /// Business request for a dynamic time
  /// </summary>
  internal sealed class DynamicTimeRequest
    : IRequest<IDynamicTimeResponse>
  {
    readonly ILog log = LogManager.GetLogger (typeof (DynamicTimeRequest).FullName);

    readonly IDynamicTimeExtension m_extension;
    readonly DateTime m_dateTime;
    readonly UtcDateTimeRange m_hint;
    readonly UtcDateTimeRange m_limit;

    /// <summary>
    /// Cache data
    /// </summary>
    internal CacheValue<IDynamicTimeResponse> CacheData { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extension">initialized extension</param>
    /// <param name="dateTime"></param>
    /// <param name="hint"></param>
    /// <param name="limit"></param>
    public DynamicTimeRequest (IDynamicTimeExtension extension, DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      Debug.Assert (null != extension.Machine);
      if (null == extension.Machine) {
        log.Error ("DynamicTimeRequest with an extension that was not initialized");
        throw new ArgumentException ("Not initialized extension", "extension");
      }
      Debug.Assert (limit.Overlaps (hint));

      m_extension = extension;
      m_dateTime = dateTime;
      m_hint = hint;
      m_limit = limit;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <returns></returns>
    public IDynamicTimeResponse Get ()
    {
      if (null != this.CacheData) {
        if (!this.CacheData.Value.Hint.Overlaps (m_limit)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Get: cache data hint {0} does not include limit {1} => no data",
              this.CacheData.Value.Hint, m_limit);
          }
          return DynamicTimeResponse.CreateNoData (this.CacheData.Value.ImplementationType);
        }
        var hint = new UtcDateTimeRange (m_hint.Intersects (this.CacheData.Value.Hint));
        if (hint.IsEmpty ()) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Get: enriched hint from {0} and cache {1} is empty",
              m_hint, this.CacheData.Value.Hint);
          }
          return DynamicTimeResponse.CreateNoData (this.CacheData.Value.ImplementationType);
        }
        var response = m_extension.Get (m_dateTime, hint, m_limit);
        if (response.NoData) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Get: no data returned for hint {0}", hint);
          }
          return DynamicTimeResponse.CreateWithHint (this.CacheData.Value.ImplementationType, hint);
        }
        else {
          return response;
        }
      }

      return m_extension.Get (m_dateTime, m_hint, m_limit);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IDynamicTimeResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"DynamicTime.{m_extension.Name}.{m_extension.Machine.Id}.{m_dateTime.ToIsoString ()}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IDynamicTimeResponse> cache)
    {
      var cacheRequest = (DynamicTimeRequest)cache.Request;
      Debug.Assert (null != cacheRequest);
      Debug.Assert (null != cacheRequest.m_limit);
      return cacheRequest.m_limit.ContainsRange (this.m_limit);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue) {
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }

      if (!m_limit.Overlaps (m_hint)) {
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }

      var cacheTimeout = m_extension.GetCacheTimeout (data);
      var maxCacheTimeout = CacheTimeOut.Permanent.GetTimeSpan ();
      if (maxCacheTimeout < cacheTimeout) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeout: {cacheTimeout} greater than max {maxCacheTimeout} => use max");
        }
        return maxCacheTimeout;
      }
      else {
        return cacheTimeout;
      }
    }
  }
}
