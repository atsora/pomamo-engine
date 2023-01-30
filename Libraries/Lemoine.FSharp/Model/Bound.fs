(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)


module Lemoine.FSharp.Model.Bound

open Lemoine.Model
open System
open FSharp.Linq

/// Bound type for F#:
/// <item>-oo</item>
/// <item>value</item>
/// <item>+oo</item>
type 'a FSharpBound =
  | MinusInfinity
  | PlusInfinity
  | Bound of 'a
  ;;

/// Convert a C# bound into a F# bound to allow the pattern matching
let ConvertToFSharpBound (bound:(IBound<'a>)) =
  if bound.HasValue then
    Bound (bound.Value)
  elif bound.BoundType.Equals(BoundType.Lower) then
    MinusInfinity
  else
    PlusInfinity
  ;;

/// Convert a C# bound into a F# bound to allow the pattern matching
let ( >>. ) = ConvertToFSharpBound;;

/// Create a lower bound
let CreateLower lowerBound = new LowerBound<'a>(lowerBound);
