(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open System.IO
open Lemoine.Stamping
open System.Threading.Tasks
open FSharp.Text.Lexing

type StampingParser(axisPropertyGetter, configuration, stampVariablesGetter) =

  let runInitializationProgram s (eventManager: ParseEventManager) =
    eventManager.StartInitialization ()
    let lexbuf = LexBuffer<_>.FromString $"O0\n{s}" in
    Parser.start Lexer.main lexbuf
    eventManager.EndInitialization ()

  member this.ParseAsync (stamper: IStamper, stampingEventHandler: IStampingEventHandler, stampingData: StampingData, cancellationToken: System.Threading.CancellationToken) =
    task {
      let subProgramParser = new SubProgramParser () in
      let lexbuf = LexBuffer<_>.FromTextReader stamper.Reader in
      let eventManager = new ParseEventManager (stamper, stampingEventHandler, stampingData, axisPropertyGetter, configuration, subProgramParser, stampVariablesGetter) in
      // Note: to initialize some g-codes you can use:
      // eventManager.InitializeGCodes (('G',20.),('G', 90.),('G', 94.))
      // But here instead configuration.InitializationGCodes is used
      Parser.eventManager <- Some (eventManager)
      match configuration.InitializationGCodes with
      | Some(gcodes) -> runInitializationProgram gcodes eventManager
      | None -> ()
      Parser.start Lexer.main lexbuf
      return true
    }

  interface IStampingParser with
    member val LineFeed = StamperLineFeed.Before with get

    member this.ParseAsync (ncProgramReaderWriter, stampingEventHandler, stampingData, cancellationToken) = this.ParseAsync (ncProgramReaderWriter, stampingEventHandler, stampingData, cancellationToken)
  
