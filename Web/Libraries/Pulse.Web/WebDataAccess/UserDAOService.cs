// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Description of UserDAO.
  /// </summary>
  public class UserDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for User/FindById
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.UserFindById request) {
      return new UserFindByIdService().Get (this.GetCacheClient(),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }
  }
}
#endif // NSERVICEKIT
