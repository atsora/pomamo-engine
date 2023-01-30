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
  /// Description of MachineStateTemplateMachineAssociationDAOServices.
  /// </summary>
  public class MachineStateTemplateMachineAssociationDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for MachineStateTemplateMachineAssociation/Save
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.MachineStateTemplateMachineAssociationSave request)
    {
      return new MachineStateTemplateMachineAssociationSaveService().Get (base.RequestContext,
                                                                          base.Request,
                                                                          request);
    }
  }
}
#endif // NSERVICEKIT
