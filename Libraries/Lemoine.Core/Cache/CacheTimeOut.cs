// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Info;
using Lemoine.Core.Log;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Cache timeout "symbolic" values
  /// </summary>
  public enum CacheTimeOut
  {
    /// <summary>
    /// current value with frequent updates
    /// </summary>
    CurrentShort,
    /// <summary>
    /// current value with rare updates
    /// </summary>
    CurrentLong,
    /// <summary>
    /// Past data with frequent updates
    /// </summary>
    PastShort,
    /// <summary>
    /// Past data with rare updates
    /// </summary>
    PastLong,
    /// <summary>
    /// Old data (older than one day) with quite frequent updates
    /// </summary>
    OldShort,
    /// <summary>
    /// Old data (older than one day) with rare updates
    /// </summary>
    OldLong,
    /// <summary>
    /// config value (stays in cache quite a bit longer than CurrentShort, CurrentLong, PastShort, PastLong)
    /// </summary>
    Config,
    /// <summary>
    /// long-lived value (won't be update very often, typically longer than Config)
    /// </summary>
    Static,
    /// <summary>
    /// Use no cache for the moment, but probably later.
    /// </summary>
    NoCache,
    /// <summary>
    /// No cache expiration
    /// </summary>
    Permanent,
  };

  /// <summary>
  /// Extensions to CacheTimeOut enum
  /// </summary>
  public static class CacheTimeOutExtensions
  {
    static readonly TimeSpan DEFAULT_CURRENT_SHORT_TIMEOUT = TimeSpan.FromSeconds (5);
    static readonly TimeSpan DEFAULT_CURRENT_LONG_TIMEOUT = TimeSpan.FromSeconds (30);
    static readonly TimeSpan DEFAULT_PAST_SHORT_TIMEOUT = TimeSpan.FromSeconds (15);
    static readonly TimeSpan DEFAULT_PAST_LONG_TIMEOUT = TimeSpan.FromSeconds (45);
    static readonly TimeSpan DEFAULT_OLD_SHORT_TIMEOUT = TimeSpan.FromSeconds (40);
    static readonly TimeSpan DEFAULT_OLD_LONG_TIMEOUT = TimeSpan.FromMinutes (2);
    static readonly TimeSpan DEFAULT_CONFIG_TIMEOUT = TimeSpan.FromHours (4);
    static readonly TimeSpan DEFAULT_STATIC_TIMEOUT = TimeSpan.FromHours (1);
    static readonly TimeSpan DEFAULT_NO_CACHE_TIMEOUT = TimeSpan.FromSeconds (0);
    static readonly TimeSpan DEFAULT_PERMANENT_TIMEOUT = TimeSpan.FromDays (1);

    static readonly ILog log = LogManager.GetLogger (typeof (CacheTimeOutExtensions).FullName);

    /// <summary>
    /// Get the associated time span to a Cache Time out
    /// </summary>
    /// <param name="cacheTimeOut"></param>
    /// <returns></returns>
    public static TimeSpan GetTimeSpan (this CacheTimeOut cacheTimeOut)
    {
      string key = "Web.Cache." + cacheTimeOut.ToString ();
      TimeSpan timeSpan = ConfigSet.LoadAndGet<TimeSpan> (key, GetDefaultTimeSpan (cacheTimeOut));
      log.DebugFormat ("GetTimeSpan: " +
                       "timeSpan for {0} is {1}",
                       cacheTimeOut, timeSpan);
      return timeSpan;
    }

    static TimeSpan GetDefaultTimeSpan (this CacheTimeOut cacheTimeOut)
    {
      switch (cacheTimeOut) {
      case CacheTimeOut.CurrentShort:
        return DEFAULT_CURRENT_SHORT_TIMEOUT;
      case CacheTimeOut.CurrentLong:
        return DEFAULT_CURRENT_LONG_TIMEOUT;
      case CacheTimeOut.PastShort:
        return DEFAULT_PAST_SHORT_TIMEOUT;
      case CacheTimeOut.PastLong:
        return DEFAULT_PAST_LONG_TIMEOUT;
      case CacheTimeOut.OldShort:
        return DEFAULT_OLD_SHORT_TIMEOUT;
      case CacheTimeOut.OldLong:
        return DEFAULT_OLD_LONG_TIMEOUT;
      case CacheTimeOut.Config:
        return DEFAULT_CONFIG_TIMEOUT;
      case CacheTimeOut.Static:
        return DEFAULT_STATIC_TIMEOUT;
      case CacheTimeOut.NoCache:
        return DEFAULT_NO_CACHE_TIMEOUT;
      case CacheTimeOut.Permanent:
        return DEFAULT_PERMANENT_TIMEOUT;
      default:
        log.FatalFormat ("GetDefaultTimeSpan: " +
                         "unexpected CacheTimeOut {0} " +
                         "=> return 0s",
                         cacheTimeOut);
        return TimeSpan.FromSeconds (0);
      }
    }
  }
}
