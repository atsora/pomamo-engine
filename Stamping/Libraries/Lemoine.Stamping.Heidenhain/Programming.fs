(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

module Lemoine.Stamping.Heidenhain.Programming

open NcProgram
open Lemoine.Core.Log
open Lemoine.Math

let log = LogManager.GetLogger ("Lemoine.Stamping.Heidenhain.Programming")

/// Position in Program Unit
type Position =
  {
    X: float option
    Y: float option
    Z: float option
  }

type Vector(x, y, z) =
  member val X = x with get, set
  member val Y = y with get, set
  member val Z = z with get, set
  new(a: Position, b: Position) =
    let dx =
      match (b.X, a.X) with
      | (Some(bx), Some(ax)) -> Some(bx - ax)
      | _ -> None
      in
    let dy =
      match (b.Y, a.Y) with
      | (Some(by), Some(ay)) -> Some(by - ay)
      | _ -> None
    in
    let dz=
      match (b.Z, a.Z) with
      | (Some(bz), Some(az)) -> Some(bz - az)
      | _ -> None
      in
    new Vector(dx, dy, dz)

  override this.ToString() =
    $"X={this.X} Y={this.Y} Z={this.Z}"

  member this.Empty with get() =
    if this.X.IsSome && (0. <> this.X.Value)
    then false
    elif this.Y.IsSome && (0. <> this.Y.Value)
    then false
    elif this.Z.IsSome && (0. <> this.Z.Value)
    then false
    else true

  member this.ToList () =
    List.map (fun (x: float option) -> x.Value) (List.filter (fun (x: float option) -> x.IsSome) [this.X; this.Y; this.Z])

  member this.Length with get() =
    computeDistanceFromPositions (this.ToList ())

  /// Scalar product
  static member (*) (a: Vector, b: Vector) =
    computeScalarProduct (a.ToList ()) (b.ToList ())

  /// Compute the angle between two vectors in radians
  static member (@@) (a: Vector, b: Vector): float option =
    try
      let a: float = computeAngleRadians (a.ToList ()) (b.ToList ()) in
      Some(a)
    with
      | ex -> None

  /// Compute the angle between two vectors in degrees
  static member (@@*) (a: Vector, b: Vector) =
    try
     let a: float = computeAngleDegrees (a.ToList ()) (b.ToList ()) in
     Some(a)
    with
      | ex -> None

let incrAxisPosition f = function
  | Some(x) -> Some(x + f)
  | None -> None

let updateCoordinate pos = function
  | ("X", Number(f)) -> { pos with X = Some(f) }
  | ("Y", Number(f)) -> { pos with Y = Some(f) }
  | ("Z", Number(f)) -> { pos with Z = Some(f) }
  | ("IX", Number(f)) -> { pos with X = (incrAxisPosition f pos.X) }
  | ("IY", Number(f)) -> { pos with Y = (incrAxisPosition f pos.Y) }
  | ("IZ", Number(f)) -> { pos with Z = (incrAxisPosition f pos.Z) }
  | ("X", _) | ("Y", _) | ("Z", _) | ("IX", _) | ("IY", _) | ("IZ", _) -> log.Warn "updateCoordinate: unknown value"; pos
  | (x, _) -> begin if log.IsTraceEnabled then log.Trace $"updateCoordinate: Unsupported axis {x}" end; pos

let rec updateCoordinates pos = function
  | [] -> pos
  | h::q -> updateCoordinates (updateCoordinate pos h) q

let inline computeAxisDistance a b =
  match (a, b) with 
  | (Some(a1), Some(a2)) -> Some(abs (a1 - a2))
  | _, _ -> None

let computeDistance p1 p2 =
  let vector = Vector(p1, p2) in
  let distance = vector.Length in
  if log.IsTraceEnabled then log.Trace $"computeDistance: p1 <-> p2 => d={distance}"
  distance

/// Get the position according to the new origin. If there is no Z in the new origin, then the Z position is skipped
let translate pos newOrigin =
  match (pos, newOrigin) with
  | ({X = Some(x); Y = Some(y); Z = Some(z)}, {X = Some(ox); Y = Some(oy); Z = Some(oz)}) -> {X = Some(x-ox); Y = Some(y-oy); Z = Some(z-oz)}
  | ({X = Some(x); Position.Y = Some(y); Z = _}, {X = Some(ox); Y = Some(oy); Z = None}) -> {X = Some(x-ox); Y = Some(y-oy); Z = None}
  | _ -> log.Error $"translate: unsupported operation for {pos} and new origin {newOrigin}"; raise (System.NotImplementedException($"Unsupported translate operation for {pos} and new origin {newOrigin}"))
  
/// Set the angle in (-PI,+PI]
let rec normalizeAngle a =
  let circleRound = 2. * System.Math.PI in
  if a < -System.Math.PI
  then normalizeAngle (circleRound + a)
  elif a >= System.Math.PI
  then normalizeAngle (a - circleRound)
  else a

let xor a b = (a || b) && not(a && b)

let roundCosinus = function
  | x when (abs x) <= 1. -> x
  | x when (1. < x) && (x < 1.01) -> 1.
  | x when  (-1.01 < x) && (x < -1.) -> -1.
  | x -> x

let computeArcDistance pos1 pos2 cc dr : float =
  match computeDistance pos1 cc with
  | 0. -> 0.
  | r ->
    begin
      let v1 = Vector(cc, pos1) in
      let v2 = Vector(cc, pos2) in
      let scalarProduct = computeScalarProduct (v1.ToList ()) (v2.ToList ()) in
      match roundCosinus (scalarProduct / (r * r)) with
      | cosangle when (abs cosangle) <= 1. -> 
        let mutable angle = acos cosangle in
        let axy1 = atan (v1.Y.Value / v1.X.Value) in
        let axy2 = atan (v2.Y.Value / v2.X.Value) in
        let axyd = normalizeAngle (axy2 - axy1) in
        if xor (axyd < 0) (dr = Clockwise)
        then angle <- 2. * System.Math.PI - angle
        r * angle
      | cosangle -> log.Error $"computeArcDistance: {cosangle} is not a valid cosinus pos1={pos1} pos2={pos2} cc={cc} => return 0."; 0.
    end

let computePathDistance f pos1 pos2 cc dr =
  match f with
  | "L" | "LN" -> computeDistance pos1 pos2
  | "C" ->
    let drx =
      match dr with
      | None -> Counterclockwise // Is it the default?
      | Some x -> x
    in
    match computeArcDistance pos1 pos2 cc drx with
    | d when System.Double.IsNaN(d) -> log.Error $"computePathDistance: arc distance is not a number"; 0.
    | d -> d
  | x -> log.Error $"computePathDistance: not supported path function {x}"; failwith (sprintf "Not supported path function %s" x)

