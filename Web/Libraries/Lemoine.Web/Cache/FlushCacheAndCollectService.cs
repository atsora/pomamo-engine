// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Cache
{
  /// <summary>
  /// Description of FlushCacheService
  /// </summary>
  public class FlushCacheAndCollectService
    : GenericNoCacheService<FlushCacheAndCollectRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FlushCacheAndCollectService).FullName);

#if !NSERVICEKIT
    readonly
#endif // NSERVICEKIT
    Lemoine.Core.Cache.ICacheClient m_cacheClient;

    #region Constructors
#if NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public FlushCacheAndCollectService ()
    {
    }
#else // !NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public FlushCacheAndCollectService (Lemoine.Core.Cache.ICacheClient cacheClient)
    {
      m_cacheClient = cacheClient;
    }
#endif // NSERVICEKIT
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(FlushCacheAndCollectRequestDTO request)
    {
      long before = GC.GetTotalMemory (true);
      FlushCache ();
      long after = GC.GetTotalMemory (true);
      
      string message = string.Format ("Cache flushed. Memory delta={0}-{1}={2} MB",
                                      before / 1024 / 1024,
                                      after / 1024 / 1024,
                                      (before-after) / 1024 / 1024);
      return new OkDTO (message);
    }
    
    /// <summary>
    /// Flush the cache
    /// </summary>
    public void FlushCache ()
    {
#if NSERVICEKIT
      if (!(this.NServiceKitCacheClient is Lemoine.Core.Cache.ICacheClient)) {
        log.FatalFormat ("GetWithoutCache: " +
                         "cache client is not a Lemoine.Core.Cache.ICacheClient");
        return;
      }
      m_cacheClient = this.NServiceKitCacheClient as Lemoine.Core.Cache.ICacheClient;
      Debug.Assert (null != m_cacheClient);
#endif // NSERVICEKIT

      m_cacheClient.FlushAll ();
      GC.Collect ();
    }
#endregion // Methods
  }
}
