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
  /// 
  /// </summary>
  public class ConfigDAOServices : Service
  {
    /// <summary>
    /// Response to GET request for Config/FindAll
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (Pulse.Web.WebDataAccess.ConfigFindAll request)
    {
      return new ConfigFindAllService ().Get (this.GetCacheClient (),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }
  }
}
#endif // NSERVICEKIT
