(*
* Copyright (C) 2023-2024 Atsora Solutions
*)

module Lemoine.FSharp.Core

let (|??>) a f =
  match a with
  | null -> null
  | x -> f x
