// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.IsoFile
{
  /// <summary>
  /// Services to iso files
  /// </summary>
  public class IsoFileServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request IsoFileSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (IsoFileSlotsRequestDTO request)
    {
      return new IsoFileSlotsService().Get (this.GetCacheClient(),
                                                         base.RequestContext,
                                                         base.Request,
                                                         request);
    }
  }
}
#endif // NSERVICEKIT
