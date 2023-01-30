// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.Modification
{
  /// <summary>
  /// Services to modification
  /// </summary>
  public class ModificationServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request OperationSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (PendingModificationsRequestDTO request)
    {
      return new PendingModificationsService().Get (this.GetCacheClient(),
                                                    base.RequestContext,
                                                    base.Request,
                                                    request);
    }
  }
}
#endif // NSERVICEKIT
