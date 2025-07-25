﻿(*
* Copyright (C) 2023-2024 Atsora Solutions
*)

module Lemoine.FSharp.Patterns

open Lemoine.FSharp.String
open System.Text.RegularExpressions

let (|Prefix|_|) (p:string) (s:string) =
  if s.StartsWith (p) then
    Some(s.Substring(p.Length))
  else
    None

(** Split a string using the characters that are found in sep *)
let (|Split|_|) (sep:string) (s:string) =
  let separators = sep.ToCharArray () in
  match s.Split (separators) with
  | [| |] -> None
  | [| _ |] -> None
  | a -> Some (Array.toSeq a)

(** Split a string using the characters that are found in sep, but only split once *)
let (|Split2|_|) (sep:string) (s:string) =
  let separators = sep.ToCharArray () in
  match s.Split (separators, 2) with
  | [| a; b |] -> Some(a, b)
  | _ -> None

(** Split a string using the string sep *)
let (|SplitS2|_|) (sep:string) (s:string) =
  match s.Split ([| sep |], 2, System.StringSplitOptions.None) with
  | [| a; b |] -> Some(a, b)
  | _ -> None

(** Split a string using the string sep, but only split once *)
let (|SplitS|_|) (sep:string) (s:string) =
  match s.Split ([| sep |], System.StringSplitOptions.None) with
  | [| |] -> None
  | [| _ |] -> None
  | a -> Some (Array.toSeq a)

let (|SingleChar|_|) (s:string) =
  if s.Length = 1 then
    Some(s[0])
  else
    None

let (|CaseInsensitive|_|) (a:string) (s:string) =
  if s.iequals a then
    Some(a)
  else
    None

let (|RegexGroup|_|) ((regex,group):Regex * string) (s:string) =
  match regex.Match s with
  | m when m.Success ->
    match m.Groups[group] with
    | g when g.Success -> Some g.Value
    | _ -> None
  | _ -> None

let (|NotNullNOrEmpty|_|) (str:string) =
  match str with
  | null | "" -> Some str
  | _ -> None

let (|NullOrEmpty|_|) (str:string) =
  match str with
  | null | "" -> Some str
  | _ -> None

let (|Integer|_|) (str:string) =
  match System.Int32.TryParse str with
  | true,i -> Some i
  | _ -> None

let (|Round|_|) (str:string) =
  match System.Double.TryParse str with
  | true,f -> Some (round f |> int)
  | _ -> None

let (|Floor|_|) (str:string) =
  match System.Double.TryParse str with
  | true,f -> Some (floor f |> int)
  | _ -> None

let (|Float|_|) (str:string) =
  match System.Double.TryParse str with
  | true,f -> Some f
  | _ -> None

let (|Boolean|_|) (str:string) =
  match System.Boolean.TryParse str with
  | true,i -> Some i
  | _ -> None
