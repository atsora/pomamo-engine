(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Core.Cache
open Lemoine.Core.Log
open Lemoine.Extensions.Web.Services
open Lemoine.Extensions.Web.Responses
open Lemoine.Model
open Lemoine.FSharp.Model.Bound
open System
open Lemoine.Extensions.Business.Group
open Lemoine.FSharp.Model.Range
open Lemoine.ModelDAO

type MilestonesGetService() =
  inherit GenericAsyncCachedService<MilestonesGetRequestDTO>(Lemoine.Core.Cache.CacheTimeOut.CurrentLong)

  let log = LogManager.GetLogger ("Lemoine.Plugin.MachiningProcessMilerstones.MilestonesGetService")

  member private this.CreateInvalidGroupDto groupId = task {
    if log.IsErrorEnabled
    then log.Error (sprintf "CreateInvalidGroupDto: group with id %s is not valid" groupId)
    let errorDto = new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter) in
    return errorDto :> obj
  }

  member private this.CreateResponseDto range (group: IGroup) = task {
    let result = new MilestonesGetResponseDTO (range) in
    let machines = group.GetMachines () in
    for machine in machines do  // TODO: parallel ?
      let! machineData = this.GetByMachine machine range in
      result.Machines.Add (machineData)
    return result :> obj
  }

  member private this.GetByMachine machine range = task {
    let byMachineResult = new MilestonesGetResponseByMachineDTO (machine) in
    let dao = new MilestoneDAO () in
    use session = ModelDAOHelper.DAOFactory.OpenSession ()
    use transaction = session.BeginReadOnlyTransaction ("Plugin.MachiningProcessMilestones.Get")
    let! milestones = dao.FindInRangeAsync machine (ConvertToFSharpRange range) in
    for milestone in milestones do
      let milestoneDto = new MilestoneDto (milestone.Id, milestone.DateTime, milestone.Message) in
      byMachineResult.Milestones.Add (milestoneDto)
    return byMachineResult
  }

  override this.Get (request:MilestonesGetRequestDTO) = task {
    let range =
      match request.Range with
      | "" | null ->
        let now = DateTime.UtcNow in
        let lowerBound = CreateLower (now.Subtract(TimeSpan.FromDays 90.0)) in
        new UtcDateTimeRange(lowerBound)
      | r -> new UtcDateTimeRange (r)
    in
    let groupId =
      match request.GroupId with
      | "" | null -> "ALL"
      | x -> x
    in
    let groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId) in
    let! group = Lemoine.Business.ServiceProvider.GetAsync (groupRequest) in
    let! result =
      match group with
      | null -> this.CreateInvalidGroupDto groupId
      | g -> this.CreateResponseDto range g in
    return result
  }

  override this.GetCacheTimeOut (url, requestDTO) =
    (* Because of the cache invalidation, the cache time could be longer *)
    match requestDTO.Range with
    | null | "" -> Lemoine.Core.Cache.CacheTimeOut.CurrentShort.GetTimeSpan ()
    | _ -> Lemoine.Core.Cache.CacheTimeOut.CurrentLong.GetTimeSpan () // TODO: if the range contains now... Invalidate...
