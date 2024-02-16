module Lemoine.FSharp.Core

let (|??>) a f =
  match a with
  | null -> null
  | x -> f x
