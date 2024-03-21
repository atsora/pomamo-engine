(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
* Copyright (C) 2024 Atsora Solutions
*
* SPDX-License-Identifier: Apache-2.0
*)

module Lemoine.StandardGCodesParser.GCode

open NcProgram
open Lemoine.Core.Log
open Lemoine.FSharp.Math

let log = LogManager.GetLogger ("Lemoine.StandardGCodesParser.GCode")

type GCode = char * float * ((char * Value) list)

type Block =
  {
    N: int Option
    GCodes: GCode list
    Parameters: (char * Value) list
    SetVariables: SetVariable list
    Comment: string option
    StampVariable: bool
    File: string option
    Escapes: float list
    Print: string option
  }

type GCodeGroup =
  | FeedRateModeGroup
  | FGroup
  | SGroup
  | TGroup
  | ToolChangeGroup
  | SpindleTurningGroup
  | CoolantGroup
  | OverrideGroup
  | DwellGroup
  | PlaneSelectionGroup
  | LengthUnitGroup
  | CutterRadiusCompensationGroup | CutterLengthCompensationGroup
  | CoordinateSystemSelectionGroup
  | PathControlModeGroup
  | DistanceModeGroup
  | RetractModeGroup
  | ReferenceLocationGroup
  | CoordinateSystemDataGroup
  | AxisOffsetsGroup
  | MotionGroup
  | StopGroup
  | LatheDiameterRadiusGroup // G07 and G08 on Hurco for example: Radius or Diameter programming
  | BlockNumberGroup
  | PositionGroup
  | HpccG05Group // Fanuc High Precision Contour Control: G05 P0 (inactive) or G05 P10000 (active)
  | AiccAiapcG051Group // Fanuc AI Contour Control and AI Advanced Preview Control: G05.1 Qx
  | AdvancedPreviewControlGroup // Fanuc: G08 P0 (inactive) or G08 P1 (active)
  | NonModalCode of char * float
  | NoSpecificGroup of char
  | UnknownGroup of int Set

exception UnknownVariableException of int

type AxisPosition =
  | Mm of float
  | In of float
  | UnknownUnit of float
  | UnknownAxisPosition

  static member Zero = UnknownUnit(0.)

  member this.IsDefined() =
    match this with
    | UnknownAxisPosition -> false
    | _ -> true

  member this.Value with get () =
    match this with
    | UnknownAxisPosition -> failwith "No value"
    | Mm(x) | In(x) | UnknownUnit(x) -> x

  member this.HasValue with get () =
    match this with
    | UnknownAxisPosition -> false
    | Mm(_) | In(_) | UnknownUnit(_) -> true

  member this.ConvertToMm() =
    match this with
    | Mm(x) -> Mm(x)
    | In(x) -> Mm(25.4 * x)
    | UnknownUnit(0.) -> UnknownUnit(0.)
    | UnknownUnit(x) -> log.Warn $"AxisPosition.ConvertToMm UnknownUnit({x})"; Mm(x)
    | UnknownAxisPosition -> UnknownAxisPosition

  member this.Map f =
    match this with
    | Mm(x) -> Mm(f x)
    | In(x) -> In(f x)
    | UnknownUnit(x) -> UnknownUnit(f x)
    | UnknownAxisPosition -> UnknownAxisPosition

  static member ApplyOp x y f d =
    match (x, y) with
    | (Mm(xmm), Mm(ymm)) -> Mm(f xmm ymm)
    | (Mm(_), In(_)) -> AxisPosition.ApplyOp x (y.ConvertToMm ()) f d
    | (In(_), Mm(_)) -> AxisPosition.ApplyOp (x.ConvertToMm ()) y f d
    | (In(xin), In(yin)) -> In(f xin yin)
    | (UnknownUnit(x), UnknownUnit(y)) -> UnknownUnit(f x y)
    | (Mm(xmm), UnknownUnit(0.)) -> Mm(f xmm 0.)
    | (Mm(xmm), UnknownUnit(y)) -> log.Warn $"AxisPosition.ApplyOp: Mm({xmm}) op UnknownUnit({y})"; Mm(f xmm y)
    | (UnknownUnit(0.), Mm(ymm)) -> Mm(f 0. ymm)
    | (UnknownUnit(x), Mm(ymm)) -> log.Warn $"AxisPosition.ApplyOp: UnknownUnit({x}) op Mm({ymm})"; Mm(f x ymm)
    | (In(xin), UnknownUnit(0.)) -> In(f xin 0.)
    | (In(xin), UnknownUnit(y)) -> log.Warn $"AxisPosition.ApplyOp: In({xin}) op UnknownUnit({y})"; In(f xin y)
    | (UnknownUnit(0.), In(yin)) -> In(f 0. yin)
    | (UnknownUnit(x), In(yin)) -> log.Warn $"AxisPosition.ApplyOp: UnknownUnit({x}) op In({yin})"; In(f x yin)
    | (UnknownAxisPosition, UnknownAxisPosition) -> UnknownAxisPosition
    | (UnknownAxisPosition, x) | (x, UnknownAxisPosition) -> d x

  static member ApplyOp2 x y f =
    match x with
    | Mm(xmm) -> Mm(f xmm y)
    | In(xin) -> In(f xin y)
    | UnknownUnit(x) -> UnknownUnit(f x y)
    | UnknownAxisPosition -> UnknownAxisPosition

  static member ApplyComparison x y f =
    match (x, y) with
    | (Mm(xmm), Mm(ymm)) -> f xmm ymm
    | (Mm(_), In(_)) -> AxisPosition.ApplyComparison x (y.ConvertToMm ()) f
    | (In(_), Mm(_)) -> AxisPosition.ApplyComparison (x.ConvertToMm ()) y f
    | (In(xin), In(yin)) -> f xin yin
    | (UnknownUnit(x), UnknownUnit(y)) -> f x y
    | (Mm(xmm), UnknownUnit(0.)) -> f xmm 0.
    | (Mm(xmm), UnknownUnit(y)) -> log.Warn $"AxisPosition.ApplyComparison: Mm({xmm}) op UnknownUnit({y})"; f xmm y
    | (UnknownUnit(0.), Mm(ymm)) -> f 0. ymm
    | (UnknownUnit(x), Mm(ymm)) -> log.Warn $"AxisPosition.ApplyComparison: UnknownUnit({x}) op Mm({ymm})"; f x ymm
    | (In(xin), UnknownUnit(0.)) -> f xin 0.
    | (In(xin), UnknownUnit(y)) -> log.Warn $"AxisPosition.ApplyComparison: In({xin}) op UnknownUnit({y})"; f xin y
    | (UnknownUnit(0.), In(yin)) -> f 0. yin
    | (UnknownUnit(x), In(yin)) -> log.Warn $"AxisPosition.ApplyComparison: UnknownUnit({x}) op In({yin})"; f x yin
    | (UnknownAxisPosition, _) | (_, UnknownAxisPosition) -> log.Warn $"AxisPosition.ApplyComparison: unknown axis position"; false

  static member (~-) (x : AxisPosition) =
    x.Map (fun a -> -1.0 * a)
  static member (+) (a, b) =
    AxisPosition.ApplyOp a b (+) (fun x -> x)
  static member (-) (a, b) =
    AxisPosition.ApplyOp a b (-) (fun x -> UnknownAxisPosition)
  static member (*) (a, b) =
    AxisPosition.ApplyOp a b (*) (fun x -> UnknownAxisPosition)
  static member (.*.) (a, b) =
    AxisPosition.ApplyOp2 a b (*)
  static member (/) (a, b) =
    AxisPosition.ApplyOp a b (/) (fun x -> UnknownAxisPosition) |> (fun x -> x.Value)
  static member (./.) (a, b) =
    AxisPosition.ApplyOp2 a b (/)
  static member op_LessThan (a, b) =
    AxisPosition.ApplyComparison a b op_LessThan
  static member op_GreaterThan (a, b) =
    AxisPosition.ApplyComparison a b op_GreaterThan
  static member op_LessThanOrEqual (a, b) =
    AxisPosition.ApplyComparison a b op_LessThanOrEqual
  static member op_GreaterThanOrEqual (a, b) =
    AxisPosition.ApplyComparison a b op_GreaterThanOrEqual
  /// Is defined?
  static member (!?) (x: AxisPosition) =
    x.IsDefined()
  static member Sqrt (x: AxisPosition): AxisPosition =
    x.Map (System.Math.Sqrt)
  /// Distance
  static member (<->) (a: AxisPosition, b: AxisPosition) =
    AxisPosition.ApplyOp a b (fun x y -> abs (x - y)) (fun x -> UnknownAxisPosition)

let inline (|*) _ y = y

type Feedrate =
  | MmMin of float
  | IPM of float
  | UnknownFeedUnit of float
  | UnknownFeedrate

type Coordinates =
  {
    X: AxisPosition
    Y: AxisPosition
    Z: AxisPosition
    I: AxisPosition
    J: AxisPosition
    K: AxisPosition
    R: AxisPosition
  }

  member this.ToXYZList () =
    List.filter (fun (x: AxisPosition) -> x.HasValue) [this.X; this.Y; this.Z]

  /// Distance
  static member (<->) (a: Coordinates, b: Coordinates) =
    let mutable d = LanguagePrimitives.GenericZero in
    match a.X <-> b.X with
    | UnknownAxisPosition -> ()
    | dx -> d <- d + dx*dx
    match a.Y <-> b.Y with
    | UnknownAxisPosition -> ()
    | dy -> d <- d + dy*dy
    match a.Z <-> b.Z with
    | UnknownAxisPosition -> ()
    | dz -> d <- d + dz*dz
    d.Map (sqrt)

type Vector(x, y, z) =
  member val X = x with get, set
  member val Y = y with get, set
  member val Z = z with get, set
  new(a: Coordinates, b: Coordinates) = new Vector(b.X - a.X, b.Y - a.Y, b.Z - a.Z)

  override this.ToString() =
    $"X={this.X} Y={this.Y} Z={this.Z}"

  member this.ToList () =
    List.filter (fun (x: AxisPosition) -> x.HasValue) [this.X; this.Y; this.Z]

  member this.Empty with get() =
    if this.X.HasValue && (0. <> this.X.Value)
    then false
    elif this.Y.HasValue && (0. <> this.Y.Value)
    then false
    elif this.Z.HasValue && (0. <> this.Z.Value)
    then false
    else true

  member this.Length with get() =
    computeDistanceFromPositions (this.ToList ())

  static member (~-) (v: Vector) =
    Vector(-v.X, -v.Y, -v.Z)

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

let getGCodeGroupOrder = function
| HpccG05Group | AiccAiapcG051Group | AdvancedPreviewControlGroup -> 0
| FeedRateModeGroup -> 0
| FGroup -> 1
| SGroup -> 2
| TGroup -> 3
| ToolChangeGroup -> 4
| SpindleTurningGroup -> 5
| CoolantGroup -> 6
| OverrideGroup -> 7
| DwellGroup -> 8
| PlaneSelectionGroup -> 9
| LengthUnitGroup -> 10
| CutterRadiusCompensationGroup -> 11
| CutterLengthCompensationGroup -> 12
| CoordinateSystemSelectionGroup -> 13
| PathControlModeGroup -> 14
| DistanceModeGroup -> 15
| RetractModeGroup -> 16
| ReferenceLocationGroup -> 17
| CoordinateSystemDataGroup -> 18
| AxisOffsetsGroup -> 19
| MotionGroup -> 20
| StopGroup -> 21
| LatheDiameterRadiusGroup -> 22
| BlockNumberGroup -> 23
| PositionGroup -> 24
| NonModalCode(_) -> 25
| NoSpecificGroup(_) -> 26
| UnknownGroup(_) -> 27

let isGCodeGroupModal = function
| FeedRateModeGroup -> true
| FGroup -> true
| SGroup -> true
| TGroup -> true
| ToolChangeGroup -> false
| SpindleTurningGroup -> true
| CoolantGroup -> true
| OverrideGroup -> true
| DwellGroup -> false
| PlaneSelectionGroup -> true
| LengthUnitGroup -> true
| CutterRadiusCompensationGroup -> true
| CutterLengthCompensationGroup -> true
| CoordinateSystemSelectionGroup -> true
| PathControlModeGroup -> true
| DistanceModeGroup -> true
| RetractModeGroup -> true
| ReferenceLocationGroup -> false
| CoordinateSystemDataGroup -> true
| AxisOffsetsGroup -> true
| MotionGroup -> true
| StopGroup -> false
| LatheDiameterRadiusGroup -> true
| BlockNumberGroup -> false
| HpccG05Group | AiccAiapcG051Group | AdvancedPreviewControlGroup -> false
| PositionGroup -> true
| NonModalCode(_) -> false
| NoSpecificGroup(_) -> false
| UnknownGroup(_) -> false

let isMotionGroup = function
| 0. | 1. | 2. | 3. | 33. | 73. | 76. -> true
| x when (80. <= x && x < 90.) -> true
| x when (38. <= x && x < 39.) -> true
| _ -> false

let gcodeToGroup variant = function
| ('G', Number(93.)) | ('G', Number(94.)) | ('G', Number(95.)) -> FeedRateModeGroup
| ('F', _) -> FGroup
| ('S', _) -> SGroup
| ('T', _) -> TGroup
| ('M', Number(6.)) -> ToolChangeGroup
| ('M', Number(3.)) | ('M', Number(4.)) | ('M', Number(5.)) -> SpindleTurningGroup
| ('M', Number(7.)) | ('M', Number(8.)) | ('M', Number(9.)) -> CoolantGroup
| ('M', Number(48.)) | ('M', Number(49.)) -> OverrideGroup
| ('G', Number(4.)) -> DwellGroup
| ('G', Number(17.)) | ('G', Number(18.)) | ('G', Number(19.)) -> PlaneSelectionGroup
| ('G', Number(20.)) | ('G', Number(21.)) -> LengthUnitGroup
| ('G', Number(40.)) | ('G', Number(41.)) | ('G', Number(42.)) -> CutterRadiusCompensationGroup
| ('G', Number(43.)) | ('G', Number(49.)) -> CutterLengthCompensationGroup
| ('G', Number(x)) when (52. <= x && x < 60.) -> CoordinateSystemSelectionGroup
| ('G', Number(x)) when (60. <= x && x < 61.) -> PathControlModeGroup
| ('G', Number(64.)) -> PathControlModeGroup
| ('G', Number(90.)) | ('G', Number(91.)) -> DistanceModeGroup
| ('G', Number(98.)) | ('G', Number(99.)) -> RetractModeGroup
| ('G', Number(28.)) | ('G', Number(30.)) -> ReferenceLocationGroup
| ('G', Number(10.)) -> CoordinateSystemDataGroup
| ('G', Number(x)) when (92. <= x && x < 93.) -> AxisOffsetsGroup
| ('G', Number(x)) when (isMotionGroup x) -> MotionGroup
| ('M', Number(0.)) | ('M', Number(1.)) | ('M', Number(2.)) | ('M', Number(30.)) | ('M', Number(60.)) -> StopGroup
| ('G', Number(7.)) | ('G', Number(8.)) when variant <> "Fanuc" -> LatheDiameterRadiusGroup
| ('G', Number(8.)) when variant = "Fanuc" -> AdvancedPreviewControlGroup
| ('G', Number(5.)) when variant = "Fanuc" -> HpccG05Group
| ('G', Number(5.1)) when variant = "Fanuc" -> AiccAiapcG051Group
| ('N', _) -> BlockNumberGroup
| ('M', Number(x)) -> NonModalCode('M', x)
| (_, Undefined(s)) -> UnknownGroup(s)
| (c, _) -> NoSpecificGroup(c)

let getGCodeOrder variant (g, x, _) =
  let group = gcodeToGroup variant (g, Number(x)) in
  getGCodeGroupOrder group

let isGCodeByDefault = function
| 'G' | 'M' | 'F' | 'S' | 'T' -> true
| _ -> false

let isAxis = function
| 'A' | 'B' | 'C' | 'U' | 'V' | 'W' | 'X' | 'Y' | 'Z' | 'I' | 'J' | 'K' | 'R' -> true
| _ -> false

let addGCode variant b c x p = { b with GCodes = (List.sortBy (getGCodeOrder variant) ((c, x, p)::b.GCodes)) }

let addParameter b c x = { b with Parameters = (c, x)::b.Parameters }

let isParameterOfGCode g x k i =
  match (g, x, k) with
  | ('G', 4., _) when i = 1 -> true
  | ('G', 5., 'P') when i = 1 -> true
  | ('G', 5., _) when 1 < i -> false
  | ('G', 5.1, 'Q') when i = 1 -> true
  | ('G', 5.1, _) when 1 < i -> true
  | ('G', 8., 'P') when i = 1 -> true
  | ('G', 8., _) when 1 < i -> false
  | ('G', 28., _) -> true
  | ('G', 30., 'P') -> true
  | ('G', 65., 'P') when i = 1 -> true
  | ('G', 65., _) when 1 < i -> true
  | ('M', 98., 'P') when i = 1 -> true
  | ('M', 98., _) when 1 < i -> true
  | ('M', 198., 'P') when i = 1 -> true
  | ('M', 198., _) when 1 < i -> true
  | (_, _, 'G') | (_, _, 'M') -> false
  | (_, _, k) when (isAxis k) -> false
  | _ -> true

exception UndefinedPositionException of int Set

let rec extractPositionFromParameters l p distanceOfFloat (configuration: Configuration) = 
  let distanceOfValue = function
  | Number x -> distanceOfFloat x
  | Undefined s -> raise (UndefinedPositionException s) in
  match l with
  | [] when configuration.IjkModal && configuration.RModal -> p: Coordinates
  | [] when configuration.IjkModal -> { p with R = UnknownAxisPosition }
  | [] when configuration.RModal -> { p with I = UnknownAxisPosition; J = UnknownAxisPosition; K = UnknownAxisPosition; R = p.R }
  | [] -> { p with I = UnknownAxisPosition; J = UnknownAxisPosition; K = UnknownAxisPosition; R = UnknownAxisPosition }
  | ('X', x)::q -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with X = distanceOfValue x }
  | ('Y', y)::q -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with Y = distanceOfValue y }
  | ('Z', z)::q -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with Z = distanceOfValue z }
  | ('U', u)::q when configuration.UvwIncremental -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with X = (distanceOfValue u) + e.X }
  | ('V', v)::q when configuration.UvwIncremental -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with Y = (distanceOfValue v) }
  | ('W', w)::q when configuration.UvwIncremental -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with Z = (distanceOfValue w) + e.Z }
  | ('I', i)::q -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with I = distanceOfValue i }
  | ('J', j)::q -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with J = distanceOfValue j }
  | ('K', k)::q -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with K = distanceOfValue k }
  | ('R', r)::q -> let e = extractPositionFromParameters q p distanceOfFloat configuration in { e with R = distanceOfValue r }
  | t::q -> extractPositionFromParameters q p distanceOfFloat configuration

exception UndefinedParameterException of char

let rec extractNamedParameter l k =
  match l with
  | [] -> raise (UndefinedParameterException k)
  | (k1, x)::q when k1 = k -> x
  | t::q -> extractNamedParameter q k

exception InvalidGroupValueException of (GCodeGroup * float)
