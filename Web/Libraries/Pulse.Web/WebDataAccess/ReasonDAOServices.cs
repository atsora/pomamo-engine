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
  /// Description of ReasonDAO.
  /// </summary>
  public class ReasonDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for Reason/FindById
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.ReasonFindById request) {
      return new ReasonFindByIdService().Get (this.GetCacheClient(),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }

    /// <summary>
    /// Response to GET request for Reason/FindByCode
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.ReasonFindByCode request) {
      return new ReasonFindByCodeService().Get (this.GetCacheClient(),
                                                base.RequestContext,
                                                base.Request,
                                                request);
    }
  }
}
#endif // NSERVICEKIT
