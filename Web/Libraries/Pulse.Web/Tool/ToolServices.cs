// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using NServiceKit.ServiceInterface;

namespace Pulse.Web.Tool
{
  /// <summary>
  /// Description of ToolServices.
  /// </summary>
  public class ToolServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request GetCurrentMachinesWithExpiredToolsService
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(CurrentMachinesWithExpiredToolsRequestDTO request)
    {
      return new CurrentMachinesWithExpiredToolsService().Get(
        this.GetCacheClient(), base.RequestContext, base.Request, request);
    }
    
    /// <summary>
    /// Response to GET request ToolLivesByMachineService
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(ToolLivesByMachineRequestDTO request)
    {
      return new ToolLivesByMachineService().Get(
        this.GetCacheClient(), base.RequestContext, base.Request, request);
    }
  }
}
#endif // NSERVICEKIT
