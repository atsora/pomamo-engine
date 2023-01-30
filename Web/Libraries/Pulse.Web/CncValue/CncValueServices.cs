// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using NServiceKit.ServiceInterface;

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Services to reasons
  /// </summary>
  public class CncValueServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request CncValueAt
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncValueAtRequestDTO request)
    {
      return new CncValueAtService().Get (this.GetCacheClient(),
                                          base.RequestContext,
                                          base.Request,
                                          request);
    }

    /// <summary>
    /// Response to GET request CncValueColor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncValueColorRequestDTO request)
    {
      return new CncValueColorService().Get (this.GetCacheClient(),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }

    /// <summary>
    /// Response to GET request CncValueCurrent
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncValueCurrentRequestDTO request)
    {
      return new CncValueCurrentService ().Get (this.GetCacheClient (),
                                                base.RequestContext,
                                                base.Request,
                                                request);
    }

    /// <summary>
    /// Response to GET request CncValueLegend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncValueLegendRequestDTO request)
    {
      return new CncValueLegendService().Get (this.GetCacheClient(),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }
    
    /// <summary>
    /// Response to GET request CncValue/RedStackLight
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (RedStackLightRequestDTO request)
    {
      return new RedStackLightService ().Get (this.GetCacheClient (),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }

  }
}
#endif // NSERVICEKIT
