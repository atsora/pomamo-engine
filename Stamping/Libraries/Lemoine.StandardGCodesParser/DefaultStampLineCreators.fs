(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open System

open Lemoine.Stamping

type DefaultStampLineCreators (stampVariablesGetter: IStampVariablesGetter) =

  member val FractionalDigits = 5 with get, set

  member this.CreateSequenceStampLine (stampId: float) =
    match stampVariablesGetter.SequenceStampVariable with
    | "" -> failwith "No sequence variable is defined"
    | v -> $"#{v}={stampId}"

  member this.CreateStartCycleStampLine (stampId: float) =
    match stampVariablesGetter.StartCycleStampVariable with
    | "" -> failwith "No start cycle variable is defined"
    | v -> $"#{v}={stampId}"

  member this.CreateStopCycleStampLine (stampId: float) =
    match stampVariablesGetter.StopCycleStampVariable with
    | "" -> failwith "No stop cycle variable is defined"
    | v -> $"#{v}={stampId}"

  member this.CreateMilestoneStampLine (sequenceTime: TimeSpan, stampId: Nullable<float>) =
    match stampVariablesGetter.MilestoneStampVariable with
    | "" -> failwith "No milestone variable is defined"
    | s ->  
      if stampId.HasValue then
        match stampVariablesGetter.SequenceStampVariable with
        | "" -> $"#{s}={sequenceTime.TotalMinutes}"
        | v -> $"#{v}={stampId.Value}\n#{s}={sequenceTime.TotalMinutes}"
      else
        $"#{s}={sequenceTime.TotalMinutes}"

  interface ISequenceStampLineCreator with
    member this.CreateSequenceStampLine (stampId: float) = this.CreateSequenceStampLine (stampId)

  interface IStartCycleStampLineCreator with
    member this.CreateStartCycleStampLine (stampId: float) = this.CreateStartCycleStampLine (stampId)

  interface IStopCycleStampLineCreator with
    member this.CreateStopCycleStampLine (stampId: float) = this.CreateStopCycleStampLine (stampId)

  interface IMilestoneStampLineCreator with
    member this.FractionalDigits
      with get (): int = this.FractionalDigits
      and set (v: int): unit = this.FractionalDigits <- v
    member this.CreateMilestoneStampLine (sequenceTime: TimeSpan, sequenceStamp: Nullable<float>) = this.CreateMilestoneStampLine (sequenceTime, sequenceStamp)
    member this.CreateResetMilestoneLine(sequenceStamp: Nullable<float>): string = this.CreateMilestoneStampLine (TimeSpan.FromSeconds (0), sequenceStamp)
