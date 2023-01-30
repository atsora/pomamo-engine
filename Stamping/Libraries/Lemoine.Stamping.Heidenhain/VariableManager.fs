(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.Stamping.Heidenhain

open System.Collections.Generic
open Lemoine.Core.Log
open System
open NcProgram

type VariableStorage(q) =

  let log = LogManager.GetLogger<VariableStorage> ()

  let variables = new Dictionary<int, Value> ()

  member this.GetVariable k =
    let v = ref (Undefined (q, k)) in
    if variables.TryGetValue (k, v) then
      v.Value
    else
      raise (UnknownVariableException((q,k)))

  member this.SetVariable k v =
    variables.Item (k) <- v;

type VariableManager() =
  let log = LogManager.GetLogger<VariableManager> ()

  let qStorage = new VariableStorage ("Q")
  let qlStorage = new VariableStorage ("QL")
  let qrStorage = new VariableStorage ("QR")
  let qsStorage = new VariableStorage ("QS")

  let getStorage = function
  | "Q" -> qStorage
  | "QL" -> qlStorage
  | "QR" -> qrStorage
  | "QS" -> qsStorage
  | x -> failwith (sprintf "Invalid Q parameter type %s" x)

  member this.Get q k =
    let storage = getStorage q in
    storage.GetVariable k

  member this.Set q k v =
    let storage = getStorage q in
    storage.SetVariable k v

  member this.Reset q k =
    let storage = getStorage q in
    storage.SetVariable k (Undefined(q,k))
