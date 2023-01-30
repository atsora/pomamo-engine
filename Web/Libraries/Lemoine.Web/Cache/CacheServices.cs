// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Lemoine.Web.Cache
{
  /// <summary>
  /// Config services
  /// </summary>
  public class CacheServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request /Cache/Flush
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(FlushCacheRequestDTO request)
    {
      return new FlushCacheService().Get(this.GetCacheClient(),
                                         base.RequestContext,
                                         base.Request,
                                         request);
    }
    
    /// <summary>
    /// Response to GET request /Cache/Flush
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(FlushCacheAndCollectRequestDTO request)
    {
      return new FlushCacheAndCollectService().Get(this.GetCacheClient(),
                                                   base.RequestContext,
                                                   base.Request,
                                                   request);
    }

    /// <summary>
    /// Response to GET request /Memory/Collect
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MemoryCollectRequestDTO request)
    {
      return new MemoryCollectService ().Get (this.GetCacheClient (),
                                         base.RequestContext,
                                         base.Request,
                                         request);
    }

  }
}
#endif // NSERVICEKIT
