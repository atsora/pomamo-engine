// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Config services
  /// </summary>
  public class ConfigServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request StringConfig
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ConfigFromKeyRequestDTO request)
    {
      return new ConfigFromKeyService ().Get (this.GetCacheClient(),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }

    /// <summary>
    /// Response to GET request /Config/NotifyUpdate
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ConfigNotifyUpdateRequestDTO request)
    {
      return new ConfigNotifyUpdateService().Get (this.GetCacheClient(),
                                                  base.RequestContext,
                                                  base.Request,
                                                  request);
    }
    
    /// <summary>
    /// Response to GET request /Config/LastUpdate
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ConfigLastUpdateRequestDTO request)
    {
      return new ConfigLastUpdateService().Get (this.GetCacheClient(),
                                                base.RequestContext,
                                                base.Request,
                                                request);
    }
  }
}
#endif // NSERVICEKIT
