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
  /// Description of MachineStateTemplateDAO.
  /// </summary>
  public class MachineStateTemplateDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for MachineStateTemplate/FindById
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.MachineStateTemplateFindById request) {
      return new MachineStateTemplateFindByIdService().Get (this.GetCacheClient(),
                                                            base.RequestContext,
                                                            base.Request,
                                                            request);
    }

    /// <summary>
    /// Response to GET request for MachineStateTemplate/FindByCategory
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.MachineStateTemplateFindByCategory request) {
      return new MachineStateTemplateFindByCategoryService().Get (this.GetCacheClient(),
                                                                  base.RequestContext,
                                                                  base.Request,
                                                                  request);
    }
  }
}
#endif // NSERVICEKIT
