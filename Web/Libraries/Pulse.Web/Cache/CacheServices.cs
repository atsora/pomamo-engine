// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;
using Lemoine.Web.Cache;

namespace Pulse.Web.Cache
{
  /// <summary>
  /// Config services
  /// </summary>
  public class CacheServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request /Cache/ClearDaySlotCache
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(ClearDaySlotCacheRequestDTO request)
    {
      return new ClearDaySlotCacheService().Get(this.GetCacheClient(),
                                                base.RequestContext,
                                                base.Request,
                                                request);
    }
    
    /// <summary>
    /// Response to GET request /Cache/ClearDomainByMachine
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(ClearDomainByMachineRequestDTO request)
    {
      return new ClearDomainByMachineService().Get(this.GetCacheClient(),
                                                   base.RequestContext,
                                                   base.Request,
                                                   request);
    }
    
    /// <summary>
    /// Response to GET request /Cache/ClearDomainByMachineModule
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(ClearDomainByMachineModuleRequestDTO request)
    {
      return new ClearDomainByMachineModuleService().Get(this.GetCacheClient(),
                                                         base.RequestContext,
                                                         base.Request,
                                                         request);
    }

    /// <summary>
    /// Response to GET request /Cache/ClearDomain
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(ClearDomainRequestDTO request)
    {
      return new ClearDomainService().Get(this.GetCacheClient(),
                                          base.RequestContext,
                                          base.Request,
                                          request);
    }
    
    /// <summary>
    /// Response to GET request /Cache/ClearPulseInfo
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(ClearPulseInfoRequestDTO request)
    {
      return new ClearPulseInfoService().Get(this.GetCacheClient(),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }
  }
}
#endif // NSERVICEKIT
