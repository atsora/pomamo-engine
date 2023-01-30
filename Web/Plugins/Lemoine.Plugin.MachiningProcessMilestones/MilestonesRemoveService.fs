(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Core.Cache
open Lemoine.Core.Log
open Lemoine.Extensions.Web.Responses
open Lemoine.Extensions.Web.Services
open Lemoine.ModelDAO
open Pulse.Web.Cache

type MilestonesRemoveService(cacheClient) =
  inherit GenericAsyncSaveService<MilestonesRemoveRequestDTO>(cacheClient)

  let log = LogManager.GetLogger ("Lemoine.Plugin.MachiningProcessMilestones.MilestonesRemoveService")

  override this.Get (request:MilestonesRemoveRequestDTO) = task {
    let id = request.Id in
    let milestoneDao = new MilestoneDAO ()
    use session = ModelDAOHelper.DAOFactory.OpenSession ()
    use transaction = session.BeginTransaction ("Plugin.MachiningProcessMilestones.Remove")
    let! milestone = milestoneDao.FindByIdAsync id in
    do! milestoneDao.MakeTransientAsync milestone
    if log.IsDebugEnabled
    then log.Debug (sprintf "GetImpl: id %d removed" milestone.Id)
    transaction.Commit ()
    let result = new MilestonesRemoveResponseDTO (milestone.Id) in
    ClearDomainService.ClearDomain (this.CacheClient, "plugin.machiningprocessmilestones", true)
    return result :> obj
  }
