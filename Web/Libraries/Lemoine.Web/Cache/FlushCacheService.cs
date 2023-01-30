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
  public class FlushCacheService
    : GenericNoCacheService<FlushCacheRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FlushCacheService).FullName);

#if !NSERVICEKIT
    readonly
#endif // NSERVICEKIT
    Lemoine.Core.Cache.ICacheClient m_cacheClient;

    #region Constructors
#if NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public FlushCacheService ()
    {
    }
#else // !NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public FlushCacheService (Lemoine.Core.Cache.ICacheClient cacheClient)
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
    public override object GetWithoutCache(FlushCacheRequestDTO request)
    {
#if NSERVICEKIT
      if (!(this.NServiceKitCacheClient is Lemoine.Core.Cache.ICacheClient)) {
        log.FatalFormat ("GetWithoutCache: " +
                         "cache client is not a Lemoine.Core.Cache.ICacheClient");
        return new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError);
      }
      m_cacheClient = this.NServiceKitCacheClient as Lemoine.Core.Cache.ICacheClient;
      Debug.Assert (null != m_cacheClient);
#endif // NSERVICEKIT
      m_cacheClient.FlushAll ();
      
      return new OkDTO ("");
    }
#endregion // Methods
  }
}
