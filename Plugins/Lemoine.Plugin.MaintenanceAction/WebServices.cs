// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Lemoine.Extensions.Web;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using NServiceKit.ServiceInterface;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Note: this class can be removed on NServiceKit is removed
  /// </summary>
  public class WebServices
    : NServiceKit.ServiceInterface.Service
  {
    public object Get (MaintenanceActionCreateRequestDTO request)
    {
      var nserviceKitCacheClient = this.GetCacheClient ();
      Lemoine.Core.Cache.ICacheClient cacheClient;
      try {
        cacheClient = NServiceKitCache.Convert (nserviceKitCacheClient);
      }
      catch (InvalidCastException) {
        return new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError);
      }
      return new NServiceKitSaveService<MaintenanceActionCreateRequestDTO> (new MaintenanceActionCreateService (cacheClient))
        .Get (base.RequestContext, base.Request, request);
    }

    public object Get (MaintenanceActionEditRequestDTO request)
    {
      var nserviceKitCacheClient = this.GetCacheClient ();
      Lemoine.Core.Cache.ICacheClient cacheClient;
      try {
        cacheClient = NServiceKitCache.Convert (nserviceKitCacheClient);
      }
      catch (InvalidCastException) {
        return new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError);
      }
      return new NServiceKitSaveService<MaintenanceActionEditRequestDTO> (new MaintenanceActionEditService (cacheClient))
        .Get (base.RequestContext, base.Request, request);
    }

    public object Get (MaintenanceActionFinishRequestDTO request)
    {
      var nserviceKitCacheClient = this.GetCacheClient ();
      Lemoine.Core.Cache.ICacheClient cacheClient;
      try {
        cacheClient = NServiceKitCache.Convert (nserviceKitCacheClient);
      }
      catch (InvalidCastException) {
        return new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError);
      }
      return new NServiceKitSaveService<MaintenanceActionFinishRequestDTO> (new MaintenanceActionFinishService (cacheClient))
        .Get (base.RequestContext, base.Request, request);
    }

    public object Get (MaintenanceActionHistoryRequestDTO request)
    {
      return new NServiceKitCachedService<MaintenanceActionHistoryRequestDTO> (new MaintenanceActionHistoryService ())
        .Get (this.GetCacheClient (),
              base.RequestContext,
              base.Request,
              request);
    }

    public object Get (MaintenanceActionNextActionsRequestDTO request)
    {
      return new NServiceKitCachedService<MaintenanceActionNextActionsRequestDTO> (new MaintenanceActionNextActionsService ())
        .Get (this.GetCacheClient (),
              base.RequestContext,
              base.Request,
              request);
    }

    public object Get (MaintenanceActionOpenMachineActionsRequestDTO request)
    {
      return new NServiceKitCachedService<MaintenanceActionOpenMachineActionsRequestDTO> (new MaintenanceActionOpenMachineActionsService ())
        .Get (this.GetCacheClient (),
              base.RequestContext,
              base.Request,
              request);
    }
  }
}
#endif // NSERVICEKIT
