(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

module Lemoine.Math

open Lemoine.Core.Log

let log = LogManager.GetLogger ("Lemoine.FSharp.Math")

/// Compute a distance from a list of axis positions
let inline computeDistanceFromPositions l =
  let rec computeSquareDistance l d =
    match l with
    | [] -> d
    | t::q -> (computeSquareDistance q d) + t*t
  in
  let sq = computeSquareDistance l LanguagePrimitives.GenericZero in
  sqrt sq

/// Compute a distance from a list of optional axis positions
let inline computeDistanceFromOptionalPositions l =
  let rec computeSquareDistance l d =
    match l with
    | [] -> d
    | Some(t)::q -> (computeSquareDistance q d) + t*t
    | None::q -> computeSquareDistance q d
  in
  let sq = computeSquareDistance l LanguagePrimitives.GenericZero in
  sqrt sq

/// Compute a vector as a list from a set of two axis positions
let inline computeVector a b =
  let rec computeVectorRec a b =
    match (a, b) with
    | ([],[]) -> []
    | (ta::qa, tb::qb) -> (tb-ta)::(computeVectorRec qa qb)
    | _ -> failwith "Lists with different sizes"
  in
  computeVectorRec a b

/// Compute a scalar product from a list of axis positions
let inline computeScalarProduct a b =
  let rec frec x y s =
    match (x, y) with
    | ([],[]) -> s
    | (ta::qa, tb::qb) -> (frec qa qb s) + ta*tb
    | _ -> failwith "Lists with different sizes"
  in
  frec a b LanguagePrimitives.GenericZero

let roundCosinus = function
| x when (abs x) <= 1. -> x
| x when (1. < x) && (x < 1.01) -> 1.
| x when  (-1.01 < x) && (x < -1.) -> -1.
| x -> x

/// Compute an angle between two vectors in radians
let inline computeAngleRadians a b =
  let scalarProduct = computeScalarProduct a b in
  let alength = computeDistanceFromPositions a in
  let blength = computeDistanceFromPositions b in
  let cosangle = roundCosinus (scalarProduct / (alength * blength)) in
  if (abs cosangle) <= 1.
  then acos cosangle
  else failwith "Invalid cosinus"

/// Compute an angle between two vectors in degrees
let inline computeAngleDegrees a b =
  let radians = computeAngleRadians a b in
  radians * 180. / System.Math.PI


[<Measure>] type mm
[<Measure>] type inch

let mmPerInch = 25.4<mm/inch>

let inchOfMm (x: float<mm>) = x / mmPerInch
let mmOfInch (x: float<inch>) = x * mmPerInch

let mmOfFloat (x: float) : float<mm> = LanguagePrimitives.FloatWithMeasure x
let inchOfFloat (x: float) : float<inch> = LanguagePrimitives.FloatWithMeasure x
