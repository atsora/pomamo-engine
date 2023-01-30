(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open FSharp.Text.Lexing
open System.IO

type SubProgramParser() =
  member this.ParseFile path =
    use textReader = File.OpenText(path)
    let lexbuf = LexBuffer<_>.FromTextReader textReader in
    Parser.start Lexer.main lexbuf

  interface ISubProgramParser with
    member this.ParseFile(path) = this.ParseFile path
