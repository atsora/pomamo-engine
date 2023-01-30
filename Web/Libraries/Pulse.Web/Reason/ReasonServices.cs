// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Services;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Services to reasons
  /// </summary>
  public class ReasonServices : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request CurrentReason
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CurrentReasonRequestDTO request)
    {
      return new CurrentReasonService ().Get (this.GetCacheClient (),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }

    /// <summary>
    /// Response to GET request ReasonAllAt
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ReasonAllAtRequestDTO request)
    {
      return new ReasonAllAtService ().Get (this.GetCacheClient (),
                                            base.RequestContext,
                                            base.Request,
                                            request);
    }

    /// <summary>
    /// Response to GET request ReasonColorSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ReasonColorSlotsRequestDTO request)
    {
      return new ReasonColorSlotsService ().Get (this.GetCacheClient (),
                                                base.RequestContext,
                                                base.Request,
                                                request);
    }

    /// <summary>
    /// Response to GET request ReasonGroupLegend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ReasonGroupLegendRequestDTO request)
    {
      return new NServiceKitCachedService<ReasonGroupLegendRequestDTO> (new ReasonGroupLegendService ())
        .Get (this.GetCacheClient (),
              base.RequestContext,
              base.Request,
              request);
    }

    /// <summary>
    /// Response to GET request ReasonOnlySlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ReasonOnlySlotsRequestDTO request)
    {
      return new ReasonOnlySlotsService ().Get (this.GetCacheClient (),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to POST request ReasonOnlySlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Post (ReasonOnlySlotsPostRequestDTO request)
    {
      return new ReasonOnlySlotsService ().Post (request,
                                                 this.Request);
    }

    /// <summary>
    /// Response to GET request for ReasonSave
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ReasonSaveRequestDTO request)
    {
      return new ReasonSaveService ().Get (base.RequestContext,
                                          base.Request,
                                          request);
    }

    /// <summary>
    /// Response to POST request ReasonSave
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Post (ReasonSavePostRequestDTO request)
    {
      return new ReasonSaveService ().Post (request,
                                            this.Request);
    }

    /// <summary>
    /// Response to GET request ReasonSelection
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ReasonSelectionRequestDTO request)
    {
      return new ReasonSelectionService ().Get (this.GetCacheClient (),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to POST request ReasonSelection
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Post (ReasonSelectionPostRequestDTO request)
    {
      return new ReasonSelectionService ().Post (request,
                                                 this.Request);
    }

    /// <summary>
    /// Response to GET request ReasonUnanswered
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ReasonUnansweredRequestDTO request)
    {
      return new ReasonUnansweredService ().Get (this.GetCacheClient (),
                                                base.RequestContext,
                                                base.Request,
                                                request);
    }
  }
}
#endif // NSERVICEKIT
