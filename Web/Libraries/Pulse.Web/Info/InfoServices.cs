// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.Info
{
  /// <summary>
  /// Info services
  /// </summary>
  public class InfoServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request PulseVersions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (PulseVersionsRequestDTO request)
    {
      return new PulseVersionsService().Get (this.GetCacheClient(),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }
    
    /// <summary>
    /// Response to GET request WebServiceAddress
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (WebServiceAddressRequestDTO request)
    {
      return new WebServiceAddressService().Get (this.GetCacheClient(),
                                                 base.RequestContext,
                                                 base.Request,
                                                 request);
    }
  }
}
#endif // NSERVICEKIT
