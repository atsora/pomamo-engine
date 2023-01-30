(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

module Lemoine.StandardGCodesParser.NcProgram

type Value = Number of float | Undefined of int Set
  with
  override this.ToString() =
    match this with
    | Number x -> x.ToString()
    | Undefined x -> "Undefined " + x.ToString()

type XCode = char * Value

type SetVariable = Value * Value

type Instruction = XCode of XCode | SetVariable of SetVariable | Comment of string | File of string | Extra of string

exception InvalidGCodeException of char

let intOfFloat (x: float): int = System.Convert.ToInt32 (x)
