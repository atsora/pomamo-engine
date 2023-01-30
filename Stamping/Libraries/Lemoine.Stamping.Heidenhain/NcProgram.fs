(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

module Lemoine.Stamping.Heidenhain.NcProgram

open System
open Lemoine.Core.Log

let log = LogManager.GetLogger ("Lemoine.Stamping.heidenhain.NcProgram")

type Unit = Mm | In

type Value = Number of float | Str of string | Undefined of (string * int) | UnknownVariables of (string * int) Set
  with
  override this.ToString() =
    match this with
    | Number x -> x.ToString()
    | Str s -> "Str " + s
    | Undefined x -> "Undefined " + x.ToString()
    | UnknownVariables x -> "UnknownVariables " + x.ToString()

type XValue = string * Value

type FeedRate = FAuto | FMax | FValue of Value

type ToolCallOption = ToolFeedRate of Value | ToolFeedUnit of string | ToolSpindleSpeed of Value | ToolOversize of XValue

type RadiusCompensation = RPLUS | RMINUS | RLEFT | RRIGHT | R0

let radiusCompensationOfString = function
  | "R+" -> RPLUS
  | "R-" -> RMINUS
  | "RL" -> RLEFT
  | "RRIGHT" -> RRIGHT
  | "R0" -> R0
  | s -> log.Fatal $"radiusCompensationOfString: invalid radius compensation {s}"; failwith (sprintf "Invalid radius compensation %s" s)

type DirectionRotation = Clockwise | Counterclockwise

let directionRotationOfString = function
  | "DR-" -> Clockwise
  | "DR+" -> Counterclockwise
  | s -> log.Fatal $"directionRotationOfString: invalid value {s}"; failwith (sprintf "Invalid direction of rotation %s" s)

type PathInstruction = { Function: string; Coordinates: XValue list; DirectionRotation: DirectionRotation option; RadiusCompensation: RadiusCompensation option; FeedRate: FeedRate option; SpindleSpeed: Value option; MCodes: float list; MCommand: (string * string list * int) option }

let createPathInstruction (f) = { Function = f; Coordinates = []; DirectionRotation = None; RadiusCompensation = None; FeedRate = None; SpindleSpeed = None; MCodes = []; MCommand = None }

let setPathFunction r f = { r with Function = f }

let addCoordinate r c = { r with Coordinates = c::r.Coordinates }

let setCoordinates r c = { r with Coordinates = c }

let setDirectionRotation r x = { r with DirectionRotation = Some(x) }

let setRadiusCompensation r x = { r with RadiusCompensation = Some(x) }

let setFeedRate r x = { r with FeedRate = Some(x) }

let setSpindleSpeed r x = { r with SpindleSpeed = Some(x) }

let addMCode r m = { r with MCodes = m::r.MCodes }

let setMCommand r m = { r with MCommand = Some(m) }

exception InvalidGCodeException of char

let intOfFloat (x: float): int = System.Convert.ToInt32 (x)

exception UnknownVariableException of (string*int)

let constToNumber = function
  "PI" -> Number(Math.PI)
  | c -> log.Error $"constToNumber: unknown constant {c}"; failwith (sprintf "Invalid constant %s" c)

type block = { LineNumber: int; StampVariable: bool }

let blockOfCommand (_,_,n) arguments stampVariable =
  match arguments with
  [] -> { LineNumber = n; StampVariable = stampVariable }
  | (_,_,_,x)::_ -> { LineNumber = x; StampVariable = stampVariable }

let blockOfMCommand (_,_,n) = { LineNumber = n; StampVariable = false }

let blockOfLineNumber n = { LineNumber = n; StampVariable = false }
