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
  /// Description of RevisionDAOServices.
  /// </summary>
  public class RevisionDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for Revision/Save
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.RevisionSave request)
    {      
      return new RevisionSaveService().Get (base.RequestContext,
                                            base.Request,
                                            request);
    }
  }
}
#endif // NSERVICEKIT
