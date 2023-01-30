(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open System.Collections.Generic
open Lemoine.Core.Log
open System
open GCode
open NcProgram

type VariableStorage() =

  let log = LogManager.GetLogger<VariableStorage> ()

  let variables = new Dictionary<int, float> ()

  member this.getVariable k =
    let v = ref 0. in
    if variables.TryGetValue (k, v) then
      v.Value
    else
      raise (UnknownVariableException(k))

  member this.setVariable k v =
    variables.Item (k) <- v;


type VariableManager() =

  let log = LogManager.GetLogger<VariableManager> ()

  let commonVariableStorage = new VariableStorage ()
  let mutable localVariableStorages = [new VariableStorage ()]

  let getStorage = function
  | k when (1 <= k && k <= 33) -> localVariableStorages.Head
  | _ -> commonVariableStorage

  let getVariable k =
    let storage = getStorage k in
    storage.getVariable k

  let setVariable k v =
     let storage = getStorage k in
     storage.setVariable k v

  let getVariableOfParameter = function
  | 'A' -> 1
  | 'B' -> 2
  | 'C' -> 3
  | 'D' -> 7
  | 'E' -> 8
  | 'F' -> 9
  | 'H' -> 11
  | 'I' -> 4
  | 'J' -> 5
  | 'K' -> 6
  | 'M' -> 13
  | 'Q' -> 17
  | 'R' -> 18
  | 'S' -> 19
  | 'T' -> 20
  | 'U' -> 21
  | 'V' -> 22
  | 'W' -> 23
  | 'X' -> 24
  | 'Y' -> 25
  | 'Z' -> 26
  | n -> failwith (sprintf "Invalid parameter %c" n)

  let pushLocalParameter k v =
    setVariable (getVariableOfParameter k) v

  member this.get k =
    let storage = getStorage k in
    storage.getVariable k

  member this.set k v =
    setVariable k v

  member this.startLocalLevel () =
    localVariableStorages <- new VariableStorage ()::localVariableStorages

  member this.endLocalLevel () =
    localVariableStorages <- localVariableStorages.Tail

  member this.pushLocalParameters = function
    | [] -> ()
    | ('P',_)::q -> this.pushLocalParameters q
    | (k,Number (v))::q -> (pushLocalParameter k v) |* (this.pushLocalParameters q)
    | (k,Undefined (_))::q -> this.pushLocalParameters q

