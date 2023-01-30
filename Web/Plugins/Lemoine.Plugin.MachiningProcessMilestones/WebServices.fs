(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

#if NSERVICEKIT

open Lemoine.Extensions.Web
open Lemoine.Extensions.Web.Responses
open Lemoine.Extensions.Web.Services
open NServiceKit.ServiceInterface

type WebServices() =
  inherit NServiceKit.ServiceInterface.Service()

  member this.Get(request: MilestonesGetRequestDTO) =
    let service = new NServiceKitCachedService<MilestonesGetRequestDTO> (new MilestonesGetService ()) in
    let cacheClient = this.GetCacheClient() in
      service.Get(cacheClient, this.RequestContext, this.Request, request)

  member this.Get(request: MilestonesSaveRequestDTO) =
    let nserviceKitCacheClient = this.GetCacheClient() in
    try
      let cacheClient = NServiceKitCache.Convert nserviceKitCacheClient in
      let service = new NServiceKitSaveService<MilestonesSaveRequestDTO> (new MilestonesSaveService (cacheClient)) in
      service.Get(this.RequestContext, this.Request, request)
    with
      | :? System.InvalidCastException -> new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError) :> obj      
      | ex -> raise ex

  member this.Get(request: MilestonesRemoveRequestDTO) =
    let nserviceKitCacheClient = this.GetCacheClient() in
    try
      let cacheClient = NServiceKitCache.Convert nserviceKitCacheClient in
      let service = new NServiceKitSaveService<MilestonesRemoveRequestDTO> (new MilestonesRemoveService (cacheClient)) in
      service.Get(this.RequestContext, this.Request, request)
    with
      | :? System.InvalidCastException -> new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError) :> obj      
      | ex -> raise ex

#endif // NSERVICEKIT
