(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.Stamping.Heidenhain

open System.Collections.Generic
open Lemoine.Core.Log
open Lemoine.Stamping
open NcProgram
open System
open Programming

type CurrentFeed = UnknownFeed | Machining of float | RapidTraverse

type ParseEventManager(ncProgramReaderWriter: IStamper, stampingEventHandler: IStampingEventHandler, stampingData: StampingData, axisPropertyGetter: IAxisPropertyGetter, configuration: Configuration) =

  let log = LogManager.GetLogger ("Lemoine.Stamping.Heidenhain.ParseEventManager")

  let unknownPosition = { X = None; Y = None; Z = None }

  let mutable programName = ""
  let mutable unit = Mm
  let mutable position = unknownPosition
  let mutable vector: Vector option = None
  let mutable circleCenter = unknownPosition
  let mutable feedrate = FAuto
  let mutable autoFeedrate = None
  let mutable spindleSpeed = None
  let mutable distance = 0.
  let mutable activeCycleNumber = None
  let mutable activeCycleParameter = None

  let variableManager = new VariableManager()

  let mmOfIn x = 25.4 * x

  let inOfMm x = x / 25.4

  let setUnit x =
    match (x, unit) with
    | (Mm, Mm) | (In, In) -> ()
    | (Mm, In) -> log.Warn "setUnit: unit change from In to Mm"; stampingData.SetLengthUnitMm(); distance <- (mmOfIn distance); unit <- x
    | (In, Mm) -> log.Warn "setUnit: unit change from Mm to In"; stampingData.SetLengthUnitIn(); distance <- (inOfMm distance); unit <- x

  let addDistance d =
    if d <> 0.
    then
      distance <- distance + d
      stampingEventHandler.SetData("Distance", d)

  let checkDirectionChange (a: float option) (b: float option) =
    if not a.IsSome || not b.IsSome
    then false
    elif a.Value = 0.
    then
      if b.Value = 0.
      then false
      else
        if log.IsTraceEnabled then log.Debug $"checkDirectionChange: from {a} to {b} => true"
        true
    elif (a.Value * b.Value) < 0
    then
      if log.IsTraceEnabled then log.Debug $"checkDirectionChange: from {a} to {b} => true"
      true
    else false

  let recordNewVector (v: Vector) =
    if vector.IsSome && (not v.Empty) && (not vector.Value.Empty) then
      if log.IsTraceEnabled then log.Trace $"recordNewVector: {vector} -> {v}"
      if checkDirectionChange vector.Value.X v.X then stampingEventHandler.SetData("DirectionChange", "X")
      if checkDirectionChange vector.Value.Y v.Y then stampingEventHandler.SetData("DirectionChange", "Y")
      if checkDirectionChange vector.Value.Z v.Z then stampingEventHandler.SetData("DirectionChange", "Z")
      match vector.Value @@* v with
      | None -> ()
      | Some angleDegrees when (abs angleDegrees) < configuration.MinRecordedAngle ->
        if log.IsTraceEnabled then log.Trace $"recordNewVector: angle {angleDegrees} is too low, lesser than {configuration.MinRecordedAngle}"
      | Some angleDegrees ->
        if log.IsTraceEnabled then log.Trace $"recordNewVector: angle {angleDegrees}"
        stampingEventHandler.SetData("Angle", angleDegrees)
    ()

  let getVariable q k =
    variableManager.Get q k

  let operatorFromString = function
  | "^" -> fun x y -> Math.Pow(x, y)
  | "*" -> (*)
  | "DIV" | "/" -> (/)
  | "%" -> (%)
  | "+" -> (+)
  | "-" -> (-)
  | "LEN" -> fun x y -> Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2))
  | "ANG" -> fun x y -> Math.Atan(x/y)
  | op -> failwith (sprintf "Unsupported operation %s" op)

  let comparisonFromString = function
  | "EQU" -> fun x y -> x = y
  | "NE" -> fun x y -> not (x = y)
  | "LT" -> fun x y -> x < y
  | "GT" -> fun x y -> x > y
  | "LE" -> fun x y -> x <= y
  | "GE" -> fun x y -> x >= y
  | op -> failwith (sprintf "Unspported comparison %s" op)

  let strOperatorFromString = function
  | "||" -> (+)
  | op -> failwith (sprintf "Unsupported string operation %s" op)

  let functionFromString = function
  | "NEG" -> fun x -> -x
  | "SQ" -> fun x -> Math.Pow(x, 2)
  | "SQRT" -> Math.Sqrt
  | "ATAN" -> Math.Atan
  | "ACOS" -> Math.Acos
  | "ASIN" -> Math.Asin
  | "COS" -> Math.Cos
  | "SIN" -> Math.Sin
  | "TAN" -> Math.Tan
  | "EXP" -> Math.Exp
  | "LN" -> Math.Log
  | "LOG" -> Math.Log10
  | "INT" -> Math.Floor
  | "ABS" -> Math.Abs
  | "FRAC" -> fun x -> x - Math.Floor(x)
  | "SGN" -> fun x -> if x = 0 then 0 elif x < 0 then -1 else +1
  | "CDATA" -> raise (NotImplementedException("CDATA which considers three points is not implemented"))
  | f -> failwith (sprintf "Unsupported function %s" f)

  let strFunctionFromString = function
  | f -> failwith (sprintf "Unsupported string function %s" f)

  let computeMachiningTime d = function
  | UnknownFeed -> TimeSpan.FromSeconds(0.)
  | Machining(0.) -> log.Fatal("computeMachiningTime: feed is 0 => infinite time"); TimeSpan.MaxValue
  | Machining(f) -> TimeSpan.FromMinutes(d / f)
  | RapidTraverse -> log.Fatal("computeMachiningTime: unexpected RapidTraverse value"); TimeSpan.FromSeconds(0.)

  let computeTimeFromVelocity d = function
    | None -> log.Warn "computeTimeFromVelocity: the velocity is null => return 0s"; TimeSpan.FromSeconds (0.)
    | Some(vx) ->
      match d with
      | Some(0.) -> TimeSpan.FromSeconds (0.)
      | Some(dx) -> TimeSpan.FromSeconds (dx / vx)
      | None ->
        begin
          log.Debug "computeTimeFromVelocity: unknown distance, sip it => return 0"
          TimeSpan.FromSeconds (0.)
        end

  let getMaxVelocity axis =
    let v = axisPropertyGetter.GetMaxVelocity(axis) in
    if v.HasValue
    then
      let u = axisPropertyGetter.GetDefaultUnit(axis) in
      match (u, unit) with
      | (AxisUnit.Mm, Mm) | (AxisUnit.In, In) | (AxisUnit.Default, _)-> Some(v.Value)
      | (AxisUnit.In, Mm) -> Some(mmOfIn v.Value) 
      | (AxisUnit.Mm, In) -> Some(inOfMm v.Value)
      | (AxisUnit.Degree, _) -> Some(v.Value)
      | (_, _) -> failwith $"Not supported axis unit {u}"
    else
      None

  let computeRapidTraverseTime { X = x1; Y = y1; Z = z1 } { X = x2; Y = y2; Z = z2 } =
    let mutable d = None in
    let mutable t = TimeSpan.FromSeconds(0.) in
    let dx = computeAxisDistance x1 x2 in
    let vx = getMaxVelocity "X" in
    let tx = computeTimeFromVelocity dx vx in
    d <- dx;
    t <- tx;
    let dy = computeAxisDistance y1 y2 in
    let vy = getMaxVelocity "Y" in
    let ty = computeTimeFromVelocity dy vy in
    if (t < ty) || (ty = t && d.IsSome && dy.IsSome && d < dy) then d <- dy; t <- ty
    let dz = computeAxisDistance z1 z2 in
    let vz = getMaxVelocity "Z" in
    let tz = computeTimeFromVelocity dz vz in
    if (t < tz) || (tz = t && d.IsSome && dz.IsSome && d < dz) then d <- dz; t <- tz
    (d, t)

  let getFeedValue = function
    | FValue(Number(v)) -> Machining(v)
    | FValue(_) -> UnknownFeed
    | FAuto ->
        match autoFeedrate with
        | Some(a) -> Machining(a)
        | None -> UnknownFeed
    | FMax -> RapidTraverse

  let getCurrentFeedValue () =
    getFeedValue feedrate

  let setDataF () =
    match getCurrentFeedValue () with
    | Machining(v) -> stampingEventHandler.SetData("PathRapidTraverse", false); stampingEventHandler.SetData("F", v)
    | UnknownFeed -> stampingEventHandler.SetData("PathRapidTraverse", false); stampingEventHandler.SetData("F", null)
    | RapidTraverse -> stampingEventHandler.SetData ("PathRapidTraverse", true)

  let setCurrentFeed f =
    match (f,unit) with
    | (FValue(Number(n)),In) -> feedrate <- FValue(Number(n/10.))
    | _ -> feedrate <- f
    setDataF ()

  let setCurrentSpindleSpeed s =
    spindleSpeed <- s
    match s with
    | Some(Number(x)) -> stampingEventHandler.SetData("S", x)
    | Some(_) -> stampingEventHandler.SetData("S", null)
    | None -> log.Error $"setCurrentSpindleSpeed: None is unexpected"; stampingEventHandler.SetData("S", null)

  let processToolCallOption = function
    | ToolFeedRate(Number(v) as f) ->
      begin
        match unit with
        | In -> autoFeedrate <- Some(v/10.)
        | _ -> autoFeedrate <- Some(v)
        setDataF ()
      end
    | _ -> ()

  let rec processToolCallOptions = function
    | [] -> ()
    | h::q -> processToolCallOption h; processToolCallOptions q

  let processMCode = function
    | 0. -> stampingEventHandler.SuspendProgram()
    | 1. -> stampingEventHandler.SuspendProgram(optional = true)
    | 2. | 30. -> stampingEventHandler.EndProgram(true, 0, false)
    | _ -> ()

  let rec processMCodes = function
    | [] -> ()
    | h::q -> processMCode h; processMCodes q

  let processMCommand mcode options =
    match mcode with
    | _ -> if log.IsTraceEnabled then log.Trace (sprintf "processMCommand: no action for m command %s" mcode)

  member this.AddBlock instructions = ()

  member this.NotifyNewBlock block =
    if log.IsTraceEnabled then log.Trace $"NotifyNewBlock: pos={block.LineNumber}"
    if block.StampVariable
    then ncProgramReaderWriter.Skip (block.LineNumber)
    else ncProgramReaderWriter.Release (block.LineNumber)
    stampingEventHandler.NotifyNewBlock(true, 0)

  member this.NotifyEndPgm pos =
    stampingEventHandler.EndProgram(true, 0, false)

  member this.NotifyEndOfFile pos =
    log.Debug $"NotifyEndOfFile: pos={pos}"
    stampingEventHandler.EndProgram(true, 0, true)
    ncProgramReaderWriter.Release()

  member this.SetData key v =
    stampingEventHandler.SetData(key, v)

  member this.ResolveVariable q k =
    try
      getVariable q k
    with
    | UnknownVariableException(x) -> UnknownVariables(Set.empty.Add(x))

  member this.SetVariable q k = function
    | Number(f) as v ->
      begin
        variableManager.Set q k v
        stampingEventHandler.SetData($"{q}{k}", f)
      end
    | Str(s) as v ->
      begin
        variableManager.Set q k v
        stampingEventHandler.SetData($"{q}{k}", s)
      end
    | Undefined(_) -> failwith (sprintf "Try to assign a %s%d from an undefined variable" q k)
    | UnknownVariables(_) -> log.Error (sprintf "SetVariable: %s%d with an undefined value" q k)

  member this.ResetVariable q k =
    variableManager.Set q k (Undefined(q,k))

  member this.IsDefined q k =
    match this.ResolveVariable q k with
    | Number(_) | Str(_) -> true
    | Undefined(_) -> false
    | UnknownVariables(_) -> failwith (sprintf "Unknown variables for IsDefined %s%d" q k)

  member this.ApplyOperator x op y =
    match (x, y) with
    | (Number(a),Number(b)) -> Number(a |> (operatorFromString op) <| b)
    | (Str(a),Str(b)) -> Str(a |> (strOperatorFromString op) <| b)
    | (Undefined(q,k), _) | (_, Undefined(q,k)) -> failwith (sprintf "Operation with an undefined variable %s%d" q k)
    | (UnknownVariables(a),UnknownVariables(b)) -> UnknownVariables(Set.union a b)
    | (_,UnknownVariables(s)) -> UnknownVariables(s)
    | (UnknownVariables(s),_) -> UnknownVariables(s)
    | _ -> failwith $"Invalid operation between {x} and {y}"

  member this.ApplyComparison x op y =
    match (x, y) with
    | (Number(a),Number(b)) -> a |> (comparisonFromString op) <| b
    | (Str(a),Str(b)) -> a |> (comparisonFromString op) <| b
    | (Undefined(_),_) -> log.Error $"ApplyComparison: {x} is undefined"; false
    | (UnknownVariables(_), _) -> log.Error $"ApplyComparison: {x} is made of unknown variables"; false
    | _ -> failwith $"Invalid comparison between {x} and {y}"

  member this.ApplyFunction f = function
    | Number(a) -> Number((functionFromString f) <| a)
    | Str(a) -> Str((strFunctionFromString f) <| a)
    | Undefined(q,k) -> Undefined(q,k)
    | UnknownVariables(s) -> UnknownVariables(s)

  member this.RecordBeginPgm = function
    | ("BEGIN PGM", name::u::[], n: int)  ->
      begin
        if log.IsDebugEnabled then log.Debug $"BEGIN PGM: name={name}"
        stampingEventHandler.StartProgram(true, 0)
        programName <- name
        match u with
        | "MM" -> setUnit Mm
        | "INCH" | "IN" -> setUnit In
        | _ -> failwith $"Invalid program unit {u}"
      end
    | ("BEGIN PGM", l, _) -> 
      begin
        log.Error $"RecordBeginPgm: invalid argument for BEGIN PGM. List length is {l.Length}"
        failwith "Invalid program name or unit for BEGIN PGM"
      end
    | (command, _, _) ->
      begin
        log.Error $"RecordBeginPgm: invalid command {command}"
        failwith (sprintf "Invalid command for RecordBeginPgm: %s" command)
      end

  member this.AddCommand c (args: (string * int * Value * int) list) =
    match c with 
    | ("END PGM", name::"MM"::[], _) | ("END PGM", name::"IN"::[], _) | ("END PGM", name::"INCH"::[], _) ->
      begin
        log.Debug "AddCommand: END PGM"
        this.NotifyEndPgm()
        false
      end
    | ("END PGM", l, _) -> 
      begin
        log.Error $"AddCommand: invalid argument for END PGM. List length is {l.Length}"
        failwith "Invalid program name or unit for END PGM"
       end
    | ("BLK FORM", name::q: string list, n: int) ->
      if log.IsTraceEnabled then log.Trace $"AddCommand: BLK FORM name={name} q={q} n={n}"
      false
    | ("TOOL DEF", _, _) -> false // TODO: just like T ? Prepare tool ?
    | ("CALL PGM", name::[], _) ->
      if log.IsTraceEnabled then log.Trace "AddCommand: CALL PGM"
      false
    | ("CALL PGM", l, _) ->
      begin
        log.Fatal $"AddCommand: invalid argument for CALL PGM. List length is {l.Length}"
        failwith "Invalid program name for CALL PGM"
       end
    | ("CYCL DEF", number::q, _) ->
      if log.IsTraceEnabled then log.Trace $"AddCommand: CYCL DEF {number} with {args.Length} arguments"
      activeCycleNumber <- Some(number)
      activeCycleParameter <- Some(List.map (fun (s,n,v,_) -> (s,n,v)) args)
      false
    | ("CYCL CALL", _, _) ->
      if log.IsTraceEnabled then log.Trace "AddCommand: CYCL CALL"
      match activeCycleNumber with
      | None ->  log.Warn $"AddCommand: no cycl def was recorded before CYCL CALL"
      | Some(c) -> this.SetData "PathFunction" $"CYCL{c}"
      match activeCycleParameter with
      | None -> ()
      | Some(l) -> this.SetData "PathParameter" (List.fold (fun p (s,n,v) -> $"{p}{s}{n}={v} ") "" l)
      false
    | ("SQL", _, _) ->
      if log.IsTraceEnabled then log.Trace $"AddComand: sql"
      false
    | ("TCH", _, _) ->
      if log.IsTraceEnabled then log.Trace $"AddComand: TCH"
      false
    | ("PLANE", _, _) ->
      if log.IsTraceEnabled then log.Trace $"AddCommand: plane"
      false
    | ("FUNCTION TCPM", _, _) ->
      if log.IsTraceEnabled then log.Trace $"AddCommand: tcpm"
      false
    | ("FUNCTION RESET", h::[], _) ->
      if log.IsTraceEnabled then log.Trace $"AddCommand: reset {h}"
      false
    | ("FUNCTION RESET", l, _) -> log.Error $"AddCommand: trying to reset {l.Length} elements"; false
    | ("ERROR", "="::l, _) ->
      if log.IsDebugEnabled then log.Debug "AddCommand: ERROR"
      false
    | (fn, format::separator::filename::[], _) when fn.StartsWith "FN 16:" ->
      if log.IsTraceEnabled then log.Trace $"AddCommand: F-PRINT {filename}"
      filename.StartsWith $"TNC:\\LEMOINE\\"
    | (x, l, _) when x.StartsWith "FN " ->
      if log.IsTraceEnabled then log.Trace $"AddCommand: diverse function {x}"
      false
    | (command, _, _) ->
      log.Error $"AddCommand: unsupported command {command}"
      false

  member this.CallTool (toolId: Value option) (axis: string option) (codes: ToolCallOption list) =
    autoFeedrate <- None
    // TODO: axis
    match toolId with
    | Some(Number(toolNumber)) ->
      begin
        if log.IsDebugEnabled then log.Debug $"CallTool: toolNumber={toolNumber}"
        stampingEventHandler.SetNextToolNumber(toolNumber.ToString())
        stampingEventHandler.TriggerToolChange()
        stampingEventHandler.SetMachiningTime (TimeSpan.FromSeconds (configuration.StandardToolChangeTime))
      end
    | Some(Str(toolName)) ->
      begin
        if log.IsDebugEnabled then log.Debug $"CallTool: toolName={toolName}"
        stampingEventHandler.SetNextToolNumber(toolName)
        stampingEventHandler.TriggerToolChange()
        stampingEventHandler.SetMachiningTime (TimeSpan.FromSeconds (configuration.StandardToolChangeTime))
      end
    | None ->
      begin
        if log.IsDebugEnabled then log.Debug $"CallTool: no tool number/name"
        stampingEventHandler.TriggerToolChange()
        stampingEventHandler.SetMachiningTime (TimeSpan.FromSeconds (configuration.StandardToolChangeTime))
      end
    | other -> log.Error $"CallTool: unsupported toolId={other}"
    processToolCallOptions codes

  member this.AddPath (m: PathInstruction) =
    match m.FeedRate with 
    | Some(f) -> setCurrentFeed f
    | None -> ()
    if m.SpindleSpeed.IsSome then setCurrentSpindleSpeed m.SpindleSpeed
    let newPosition = updateCoordinates position m.Coordinates in
    if log.IsTraceEnabled then log.Trace $"AddPath: position old={position} new={newPosition} coordinates={m.Coordinates}"
    match feedrate with
    | FValue(Number(_)) | FAuto ->
      let f = getCurrentFeedValue () in
      let d = computePathDistance m.Function position newPosition circleCenter m.DirectionRotation in
      addDistance d
      let time = computeMachiningTime d f in
      if log.IsTraceEnabled then log.Trace $"AddPath: f={f} distance={d} time={time}"
      stampingEventHandler.TriggerMachining ()
      stampingEventHandler.SetMachiningTime (time)
    | FValue(_) -> ()
    | FMax ->
      let (d, time) = computeRapidTraverseTime position newPosition in
      if d.IsSome then addDistance d.Value
      stampingEventHandler.SetMachiningTime (time)
    if position <> newPosition then
      let newVector = Vector (position, newPosition) in
      if vector.IsSome then recordNewVector newVector
      position <- newPosition
      if newVector.Empty then log.Error $"AddBlock: newVector {newVector} is empty"
      vector <- Some newVector
    processMCodes m.MCodes
    match m.MCommand with
    | None -> ()
    | Some(mcode,options,_) -> processMCommand mcode options
    this.SetData "PathFunction" m.Function

  member this.SetCircleCenter coordinates =
    circleCenter <- position
    circleCenter <- updateCoordinates circleCenter coordinates

  member this.CallLbl (lbl,rep) =
    ()

  member this.RecordLbl lbl =
    ()

  member this.NotifyComment comment =
    stampingEventHandler.SetComment(comment)

  member this.RecordLineNumber lineNumber =
    if log.IsTraceEnabled then log.Trace (sprintf "RecordLineNumber: %d" lineNumber)

  member this.AddStop = function
    | 0 -> stampingEventHandler.SuspendProgram()
    | 1 -> stampingEventHandler.SuspendProgram(optional = true)
    | 2 | 30 -> stampingEventHandler.EndProgram(true, 0, false)
    | 6 -> stampingEventHandler.TriggerToolChange(); stampingEventHandler.SuspendProgram()
    | x -> log.Warn (sprintf "AddStop: unexpected M Code %d after STOP" x)
