// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Services to production states
  /// </summary>
  public class ProductionStateServices : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ProductionRateRequestDTO request)
    {
      return new ProductionRateService ().Get (this.GetCacheClient (),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ProductionStateColorSlotsRequestDTO request)
    {
      return new ProductionStateColorSlotsService ().Get (this.GetCacheClient (),
                                                          base.RequestContext,
                                                          base.Request,
                                                          request);
    }

    /// <summary>
    /// Response to GET request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ProductionStateLegendRequestDTO request)
    {
      return new ProductionStateLegendService ().Get (this.GetCacheClient (),
                                                      base.RequestContext,
                                                      base.Request,
                                                      request);
    }

    /// <summary>
    /// Response to GET request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ProductionStateSlotsRequestDTO request)
    {
      return new ProductionStateSlotsService ().Get (this.GetCacheClient (),
                                                     base.RequestContext,
                                                     base.Request,
                                                     request);
    }
  }
}
#endif // NSERVICEKIT
