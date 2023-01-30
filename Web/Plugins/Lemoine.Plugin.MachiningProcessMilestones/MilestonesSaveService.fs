(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Collections
open Lemoine.Core.Log
open Lemoine.Extensions.Web.Responses
open Lemoine.Extensions.Web.Services
open Lemoine.Model
open Lemoine.FSharp.Model.Bound
open System
open System.Linq
open Lemoine.ModelDAO
open Pulse.Web.Cache

type MilestonesSaveService(cacheClient) =
  inherit GenericAsyncSaveService<MilestonesSaveRequestDTO>(cacheClient)

  let log = LogManager.GetLogger ("Lemoine.Plugin.MachiningProcessMilestones.MilestonesSaveService")

  member private this.CreateInvalidGroupDto groupId = task {
    if log.IsErrorEnabled
    then log.Error (sprintf "CreateInvalidGroupDto: group with id %s is not valid" groupId)
    let errorDto = new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter) in
    return errorDto :> obj
  }

  member private this.CreateNotSupportedGroupDto groupId details = task {
    if log.IsErrorEnabled
    then log.Error (sprintf "CreateNotSupportedGroupDto: group with id %s is not supported" groupId)
    let errorDto = new ErrorDTO (sprintf "Not supported group (%s)" details, ErrorStatus.WrongRequestParameter) in
    return errorDto :> obj
  }

  member private this.CreateResponseDto machine dateTime message = task {
    let milestone = new Milestone (machine, dateTime, message) in
    let milestoneDao = new MilestoneDAO ()
    use session = ModelDAOHelper.DAOFactory.OpenSession ()
    use transaction = session.BeginTransaction ("Plugin.MachiningProcessMilestones.Save")
    let! milestone = milestoneDao.MakePersistentAsync milestone in
    if log.IsDebugEnabled
    then log.Debug (sprintf "CreateResponseDto: new id %d" milestone.Id)
    let result = new MilestonesSaveResponseDTO (milestone.Id) in
    transaction.Commit ()
    return result :> obj
  }

  override this.Get (request:MilestonesSaveRequestDTO) = task {
    let groupId = request.GroupId in
    let dateTime = (ConvertDTO.IsoStringToDateTimeUtc request.At).Value in
    let groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId) in
    let! group = Lemoine.Business.ServiceProvider.GetAsync (groupRequest) in
    let! result =
      match group with
      | null -> this.CreateInvalidGroupDto groupId
      | g when (not g.SingleMachine) -> this.CreateNotSupportedGroupDto groupId "multi-machines"
      | g -> let machine = g.GetMachines () |> Seq.head in this.CreateResponseDto machine dateTime request.Message in
    ClearDomainService.ClearDomain (this.CacheClient, "plugin.machiningprocessmilestones", true)
    return result
  }
