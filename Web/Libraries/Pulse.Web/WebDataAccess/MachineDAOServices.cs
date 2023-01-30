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
  /// Description of MachineDAO.
  /// </summary>
  public class MachineDAOServices : Service
  {
    /// <summary>
    /// Response to GET request for Machine/FindById
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (Pulse.Web.WebDataAccess.MachineFindById request)
    {
      return new MachineFindByIdService ().Get (this.GetCacheClient (),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }
  }
}
#endif // NSERVICEKIT
