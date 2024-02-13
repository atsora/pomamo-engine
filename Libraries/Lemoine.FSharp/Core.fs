module Lemoine.Core

let (|??>) a f =
  match a with
  | null -> null
  | x -> f x
