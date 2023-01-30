(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)


module Lemoine.FSharp.Model.Range

open Lemoine.FSharp.Model.Bound
open Lemoine.Model
open System

/// Type of F# range to allow the pattern matching:
/// <item>empty</item>
/// <item>other made of bounds</item>
type 'a FSharpRange =
  | Empty
  | Bounds of ('a FSharpBound * bool * 'a FSharpBound * bool)
  ;;

/// Convert a C# range into a F# range to allow the pattern matching
let ConvertToFSharpRange (r:IRange<'a>) =
  if r.IsEmpty() then
    Empty
  else
    Bounds ((ConvertToFSharpBound r.Lower), r.LowerInclusive, ConvertToFSharpBound r.Upper, r.UpperInclusive)
  ;;

/// Convert a C# range into a F# range to allow the pattern matching
let ( >>@ ) = ConvertToFSharpRange;;
