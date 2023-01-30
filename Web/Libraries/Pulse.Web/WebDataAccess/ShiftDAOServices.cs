// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

#if NSERVICEKIT
using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Description of ShiftDAO.
  /// </summary>
  public class ShiftDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for Shift/FindById
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.ShiftFindById request) {
      return new ShiftFindByIdService().Get (this.GetCacheClient(),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }
  }
}
#endif // NSERVICEKIT
