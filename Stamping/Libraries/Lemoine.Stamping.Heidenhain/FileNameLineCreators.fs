namespace Lemoine.Stamping.Heidenhain

(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

open System

open Lemoine.Stamping

/// The new method to get a value is based on filename.
/// A dedicated folder in TNC:\LEMOINE folder will be created for each variable, and in this folder, files will be created with as filename the variable value.
///
/// The method "GetValueFromFileName" will get the value from the filename of the file with the latest timestamp
///
/// In NC programs, values are written by creating a file in the dedicated folder with value as the filename. For each value create a file using the value
/// FN 16: F-PRINT TNC:\LEMOINE\<folder>\<value>
type FileNameLineCreators (stampVariablesGetter: IStampVariablesGetter) =

  let createLine k v = $@"FN 16: F-PRINT TNC:\LEMOINE\{k}\{v}"
 
  member val FractionalDigits = 0 with get, set

  member this.CreateSequenceStampLine (stampId: float) =
    match stampVariablesGetter.SequenceStampVariable with
    | "" -> failwith "No sequence variable is defined"
    | v -> createLine v stampId

  member this.CreateStartCycleStampLine (stampId: float) =
    match stampVariablesGetter.StartCycleStampVariable with
    | "" -> failwith "No start cycle variable is defined"
    | v -> createLine v stampId

  member this.CreateStopCycleStampLine (stampId: float) =
    match stampVariablesGetter.StopCycleStampVariable with
    | "" -> failwith "No stop cycle variable is defined"
    | v -> createLine v stampId

  member this.CreateMilestoneStampLine (sequenceTime: TimeSpan, stampId: Nullable<float>) =
    match stampVariablesGetter.MilestoneStampVariable with
    | "" -> failwith "No milestone variable is defined"
    | s ->  
      if stampId.HasValue then
        match stampVariablesGetter.SequenceStampVariable with
        | "" -> createLine s sequenceTime.TotalMinutes
        | v -> $"{createLine v stampId.Value}\n{createLine s (Math.Round(sequenceTime.TotalMinutes, this.FractionalDigits))}"
      else
        createLine s (Math.Round(sequenceTime.TotalMinutes, this.FractionalDigits))

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
    member this.CreateResetMilestoneLine(sequenceStamp: Nullable<float>): string =  this.CreateMilestoneStampLine (TimeSpan.FromSeconds (0), sequenceStamp)
