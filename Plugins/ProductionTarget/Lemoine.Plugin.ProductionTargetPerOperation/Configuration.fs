(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)


namespace Lemoine.Plugin.ProductionTargetPerOperation

open System.Collections.Generic
open System.ComponentModel

open Lemoine.Core.Log
open Lemoine.Extensions.Configuration.GuiBuilder
open Lemoine.ModelDAO

type Configuration() =

  [<PluginConf ("Operation", "Operation", Description = "specific operation to target", Multiple = false, Optional = false)>]
  member val OperationId = 0 with get, set

  [<PluginConf ("Double", "Score", Description = "Score to give to this extension", Multiple = false, Optional = true)>]
  [<DefaultValue (100.0)>]
  member val Score = 100.0 with get, set

  [<PluginConf ("Double", "Target", Description = "Target per hour", Multiple = false, Optional = false)>]
  [<DefaultValue (0.0)>]
  member val TargetPerHour = 0.0 with get, set

  interface Lemoine.Extensions.Configuration.IConfiguration with
    member this.IsValid(errors: byref<System.Collections.Generic.IEnumerable<string>>) = 
      let errorList = new List<string> () in
      begin
        match this.OperationId with
        | 0 -> errorList.Add "OperationId is mandatory"
        | x ->
          begin
            use session = ModelDAOHelper.DAOFactory.OpenSession ()
            use transaction = session.BeginReadOnlyTransaction ("Plugin.ProductionTargetPerOperation.Configuration")
            let operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById this.OperationId in
            match operation with
            | null -> errorList.Add (sprintf "Operation %d does not exist" this.OperationId)
            | _ -> ()
          end
        errors <- errorList;
        0 = errorList.Count
      end
