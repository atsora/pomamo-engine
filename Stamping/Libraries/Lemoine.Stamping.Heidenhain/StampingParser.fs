(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.Stamping.Heidenhain

open System.IO
open Lemoine.Stamping
open System.Threading.Tasks
open FSharp.Text.Lexing

type StampingParser(axisPropertyGetter, configuration) =

  member this.ParseAsync (stamper: IStamper, stampingEventHandler: IStampingEventHandler, stampingData: StampingData, cancellationToken: System.Threading.CancellationToken) =
    task {
      let lexbuf = LexBuffer<_>.FromTextReader stamper.Reader in
      let eventManager = new ParseEventManager (stamper, stampingEventHandler, stampingData, axisPropertyGetter, configuration) in
      Parser.eventManager <- Some (eventManager)
      Parser.start Lexer.main lexbuf
      return true
    }

  interface IStampingParser with
    member val LineFeed = StamperLineFeed.Before with get

    member this.ParseAsync (ncProgramReaderWriter, stampingEventHandler, stampingData, cancellationToken) = this.ParseAsync (ncProgramReaderWriter, stampingEventHandler, stampingData, cancellationToken)
  
