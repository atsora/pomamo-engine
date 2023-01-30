(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open System.Threading.Tasks

open Lemoine.Core.Log
open Lemoine.Core.Cache
open Lemoine.Extensions.Business.Cache

type CacheDomainExtension() =
  let log = LogManager.GetLogger ("Lemoine.Plugin.MachiningProcessMilestones.CacheDomainExtension")

  member val UniqueInstance = true

  member this.ClearDomain (cacheClient: ICacheClient, domain: string) =
    match domain.ToLowerInvariant () with
    | "plugin.machiningprocessmilestones" -> cacheClient.ClearDomainFromRegexes [| "Milestones.*" |]
    | _ -> false

  interface Lemoine.Extensions.IExtension with
    member this.UniqueInstance = this.UniqueInstance

  interface Lemoine.Extensions.Business.Cache.ICacheDomainExtension with
    member this.ClearDomain (cacheClient, domain) = this.ClearDomain (cacheClient, domain)
    member this.ClearDomainByMachine (cacheClient, domain, machineId) = false
    member this.ClearDomainByMachineModule (cacheClient, domain, machineModuleId) = false
    member this.ClearDomainAsync (cacheClient, domain) =
      task {
        return this.ClearDomain (cacheClient, domain)
      }      
    member this.ClearDomainByMachineAsync (cacheClient, domain, machineId) =
      task {
        return false
      }      
    member this.ClearDomainByMachineModuleAsync (cacheClient, domain, machineModuleId) =
      task {
        return false
      }
