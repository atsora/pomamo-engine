﻿(*
* Copyright (C) 2023-2024 Atsora Solutions
*)

module Lemoine.FSharp.String

let nullorempty s =
  System.String.IsNullOrEmpty s

type System.String with
  /// Case insensitive equality for strings
  member s1.iequals(s2: string) =
    System.String.Equals (s1, s2, System.StringComparison.InvariantCultureIgnoreCase)
