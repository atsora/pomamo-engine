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

    readonly Lemoine.Core.Cache.ICacheClient m_cacheClient;

    /// <summary>
    /// 
    /// </summary>
    public FlushCacheAndCollectService (Lemoine.Core.Cache.ICacheClient cacheClient)
    {
      m_cacheClient = cacheClient;
    }

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
      m_cacheClient.FlushAll ();
      GC.Collect ();
    }
#endregion // Methods
  }
}
