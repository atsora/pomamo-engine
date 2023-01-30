// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Description of ComputerDAO.
  /// </summary>
  public class ComputerDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for Data/Computer/GetLctr
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.ComputerGetLctr request) {
      return new ComputerGetLctrService().Get (this.GetCacheClient(),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request for Data/Computer/GetLposts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.ComputerGetLposts request) {
      return new ComputerGetLpostsService().Get (this.GetCacheClient(),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request for Data/Computer/GetCnc
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.ComputerGetCnc request) {
      return new ComputerGetCncService().Get (this.GetCacheClient(),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request for Data/Computer/GetWeb
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.ComputerGetWeb request) {
      return new ComputerGetWebService().Get (this.GetCacheClient(),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request for Data/Computer/GetAutoReason
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (Pulse.Web.WebDataAccess.ComputerGetAutoReason request)
    {
      return new ComputerGetAutoReasonService ().Get (this.GetCacheClient (),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request for Data/Computer/GetAlert
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (Pulse.Web.WebDataAccess.ComputerGetAlert request)
    {
      return new ComputerGetAlertService ().Get (this.GetCacheClient (),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request for Data/Computer/GetSynchronization
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (Pulse.Web.WebDataAccess.ComputerGetSynchronization request)
    {
      return new ComputerGetSynchronizationService ().Get (this.GetCacheClient (),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

  }
}
#endif // NSERVICEKIT
