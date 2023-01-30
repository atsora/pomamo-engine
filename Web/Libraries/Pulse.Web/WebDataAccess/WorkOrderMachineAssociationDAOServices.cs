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
  /// Description of WorkOrderMachineAssociationDAOServices.
  /// </summary>
  public class WorkOrderMachineAssociationDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for WorkOrderMachineAssociation/Save
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.WorkOrderMachineAssociationSave request)
    {
      return new WorkOrderMachineAssociationSaveService().Get (base.RequestContext,
                                                            base.Request,
                                                            request);
    }
  }
}
#endif // NSERVICEKIT
