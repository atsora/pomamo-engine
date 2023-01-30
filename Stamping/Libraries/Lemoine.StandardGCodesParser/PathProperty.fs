(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open Lemoine.Model
open Lemoine.Stamping
open System.Runtime.InteropServices
open System

type PathProperty(stamper, stampingData: StampingData) =
  
  let mutable gmotion = None

  member val Next: IStampingEventHandler = null with get, set

  member this.NotifyNewBlock(edit: bool, level: int) =
    match this.Next with
    | null -> ()
    | n -> n.NotifyNewBlock(edit, level)

  member this.SetData(key, v: Object) =
    let lengthUnit = stampingData.GetLengthUnit() in
    match (key, v) with
    | ("G-ReferenceLocation", x) -> this.SetData("PathFunction", $"G{x}"); this.SetData("PathRapidTraverse", true)
    | ("G-Motion", x) when x = 0. -> gmotion <- Some(x); this.SetData("PathFunction", "G0"); this.SetData("PathRapidTraverse", true)
    | ("G-Motion", x) -> gmotion <- Some(x); this.SetData("PathFunction", $"G{x}"); this.SetData("PathRapidTraverse", false)
    | ("F", x) -> this.SetData("PathFeedRate", x)
    | ("G-FeedRateMode", x) when x = 93. -> this.SetData("Unit", MachiningUnit.InverseTime ||| lengthUnit)
    | ("G-FeedRateMode", x) when x = 95. -> this.SetData("Unit", MachiningUnit.RevolutionMin ||| lengthUnit)
    | ("G-FeedRateMode", x) -> this.SetData("Unit", lengthUnit)
    | ("S", x) -> this.SetData("PathSpindleSpeed", x)
    | ("Macro", x) ->
      this.SetData("PathFunction", $"Macro{x}");
      this.SetData("PathNonMachining", true)
    | ("Distance", _) -> if gmotion.IsSome then this.SetData("G-Motion", gmotion.Value)
    | _ -> ()
    match this.Next with
    | null -> ()
    | n -> n.SetData(key, v)

  member this.TriggerToolChange(toolNumber) =
    this.SetData("PathFunction", "ToolChange")
    this.SetData("PathNonMachining", true)
    match this.Next with
    | null -> ()
    | n -> n.TriggerToolChange(toolNumber)

  interface IStampingEventHandler with
    member this.Next with get () = this.Next and set v = this.Next <- v

    member val Stamper = stamper with get

    member this.NotifyNewBlock(edit, level) = this.NotifyNewBlock(edit, level)

    member this.SetComment(message) =
      match this.Next with
      | null -> ()
      | n -> n.SetComment(message)

    member this.SetData(key, v) = this.SetData(key, v)

    member this.SetMachiningTime(duration) =
      match this.Next with
      | null -> ()
      | n -> n.SetMachiningTime(duration)

    member this.SetNextToolNumber(toolNumber) =
      match this.Next with
      | null -> ()
      | n -> n.SetNextToolNumber(toolNumber)

    member this.StartCycle() =
      match this.Next with
      | null -> ()
      | n -> n.StartCycle()

    member this.StartSequence(sequenceKind) =
      match this.Next with
      | null -> ()
      | n -> n.StartSequence(sequenceKind)

    member this.StopCycle() =
      match this.Next with
      | null -> ()
      | n -> n.StopCycle()

    member this.StartProgram(edit, level) =
      match this.Next with
      | null -> ()
      | n -> n.StartProgram(edit, level)

    member this.EndProgram(edit, level, endOfFile) =
      match this.Next with
      | null -> ()
      | n -> n.EndProgram(edit, level, endOfFile)

    member this.ResumeProgram(edit, level) =
      match this.Next with
      | null -> ()
      | n -> n.ResumeProgram(edit, level)

    member this.SuspendProgram([<Optional; DefaultParameterValue(false)>] optional, [<OptionalArgument; DefaultParameterValue("")>] details) =
      match this.Next with
      | null -> ()
      | n -> n.SuspendProgram(optional, details)

    member this.TriggerMachining() =
      match this.Next with
      | null -> ()
      | n -> n.TriggerMachining()

    member this.TriggerToolChange(toolNumber) = this.TriggerToolChange(toolNumber)
