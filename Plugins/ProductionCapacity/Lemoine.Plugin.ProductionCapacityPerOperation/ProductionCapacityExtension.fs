(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.ProductionCapacityPerOperation

open System.Collections.Generic

open Lemoine.Core.Log
open Lemoine.Core.Cache
open Lemoine.Extensions.Business.Cache
open Lemoine.Model
open Lemoine.ModelDAO

type ProductionCapacityExtension () =
  inherit Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>()

  let log = LogManager.GetLogger ("Lemoine.Plugin.ProductionCapacityPerOperation.ProductionCapacityExtension")

  let mutable operation: IOperation = null
  let intermediateWorkPieces = new List<IIntermediateWorkPiece> ()
  let mutable configuration: Configuration = new Configuration ()

  interface Lemoine.Extensions.IExtension with
    member this.UniqueInstance = this.UniqueInstance

  interface Lemoine.Extensions.Business.Operation.IProductionCapacityExtension with
    member this.Initialize(machine) =
      match this.LoadConfiguration (&configuration) with
      | false -> false
      | true ->
        begin
          use session = ModelDAOHelper.DAOFactory.OpenSession ()
          use transaction = session.BeginReadOnlyTransaction ("Plugin.ProductionCapacityPerOperation.ProductionCapacityExtension")
          let operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById configuration.OperationId in
          if isNull operation
          then log.Error (sprintf "Operation with id=%d does not exist" configuration.OperationId) |> fun () -> false
          else
            begin
              for intermediateWorkPiece in operation.IntermediateWorkPieces do
                intermediateWorkPieces.Add intermediateWorkPiece;
              true
            end
        end

    member this.GetCapacityPerHourAsync(intermediateWorkPiece: IIntermediateWorkPiece): System.Threading.Tasks.Task<float> = 
      task {
        if Seq.exists (fun (iwp: IIntermediateWorkPiece) -> iwp.Id = intermediateWorkPiece.Id) intermediateWorkPieces
        then return configuration.CapacityPerHour
        else return raise <| new System.Exception "Not supported intermediate work piece"
      }

    member this.Score = configuration.Score
