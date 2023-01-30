// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Services;
using NServiceKit.ServiceInterface;

namespace Lemoine.Plugin.ProductionTracker
{
  /// <summary>
  /// Note: this class can be removed once NServiceKit is removed
  /// </summary>
  public class Services
    : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request for EmailSend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ProductionTrackerRequestDTO request)
    {
      return new NServiceKitCachedService<ProductionTrackerRequestDTO> (new ProductionTrackerService ())
        .Get (this.GetCacheClient (),
              base.RequestContext,
              base.Request,
              request);
    }
  }
}
#endif // NSERVICEKIT
