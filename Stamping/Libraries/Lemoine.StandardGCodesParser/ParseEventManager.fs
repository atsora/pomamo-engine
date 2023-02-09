(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open System.Collections.Generic
open Lemoine.Core.Log
open Lemoine.Math
open Lemoine.Stamping
open NcProgram
open GCode
open System

type LengthUnit =
  | LengthUnitUnknown
  | LengthMm
  | LengthIn

type ParseEventManager(ncProgramReaderWriter: IStamper, stampingEventHandler: IStampingEventHandler, stampingData: StampingData, axisPropertyGetter: IAxisPropertyGetter, configuration: Configuration, subProgramParser: ISubProgramParser, stampVariablesGetter: IStampVariablesGetter) =

  let log = LogManager.GetLogger ("Lemoine.StandardGCodesParser.ParseEventManager")

  let variableManager = new VariableManager ()

  let modalGroupValues = new ModalGroupValues ()

  let unknownCoordinates: Coordinates = { X = UnknownAxisPosition; Y = UnknownAxisPosition; Z = UnknownAxisPosition; I = UnknownAxisPosition; J = UnknownAxisPosition; K = UnknownAxisPosition; R = UnknownAxisPosition }

  let mutable position = unknownCoordinates
  let mutable vector: Vector option = None
  let mutable initialization = false
  let mutable callLevel = 0
  let mutable edit = [true]
  let mutable unit = LengthUnitUnknown
  let mutable distance = 0.
  let mutable blockNumber = 0
  let mutable headerSection = true

  let mmOfIn x = 25.4 * x

  let inOfMm x = x / 25.4

  let setUnitLength x =
    match (x, unit) with
    | (LengthUnitUnknown, _) -> log.Error "setUnitLength: setting a new unit unknown, which is unexpected"; ()
    | (LengthMm, LengthMm) | (LengthIn, LengthIn) -> ()
    | (LengthMm, LengthIn) -> log.Warn "setUnitLength: unit change from In to Mm"; stampingData.SetLengthUnitMm(); distance <- (mmOfIn distance); unit <- x
    | (LengthIn, LengthMm) -> log.Warn "setUnitLength: unit change from Mm to In"; stampingData.SetLengthUnitIn(); distance <- (inOfMm distance); unit <- x
    | (LengthMm, LengthUnitUnknown) -> stampingData.SetLengthUnitMm(); unit <- x
    | (LengthIn, LengthUnitUnknown) -> stampingData.SetLengthUnitIn(); unit <- x

  let addDistance d =
    match (d, unit) with
    | (Mm(0.),_) | (In(0.),_) | (UnknownUnit(0.),_) -> ()
    | (Mm(x), LengthMm) | (In(x), LengthIn) ->
      distance <- distance + x
      stampingEventHandler.SetData("Distance", x)
    | (In(x), LengthMm) ->
      log.Warn $"addDistance: add a length in In {x} while the set length is in Mm"
      let dmm = mmOfIn x in
      distance <- distance + dmm
      stampingEventHandler.SetData("Distance", dmm)
    | (Mm(x), LengthIn) ->
      log.Warn $"addDistance: add a length in Mm {x} while the set length is in In"
      let din = inOfMm x in
      distance <- distance + din
      stampingEventHandler.SetData("Distance", din)
    | (Mm(x), LengthUnitUnknown) ->
      log.Warn $"addDistance: add a length in Mm {x} while the set length is unknown"
      distance <- distance + x
      stampingEventHandler.SetData("Distance", x)
    | (In(x), LengthUnitUnknown) ->
      log.Warn $"addDistance: add a length in In {x} while the set length is unknown"
      distance <- distance + x
      stampingEventHandler.SetData("Distance", x)
    | (UnknownUnit(x), _) ->
      distance <- distance + x
      stampingEventHandler.SetData("Distance", x)
    | (UnknownAxisPosition, _) -> ()

  let checkDirectionChange (a: AxisPosition) (b: AxisPosition) =
    if not a.HasValue || not b.HasValue
    then false
    elif a.Value = 0.
    then
      if b.Value = 0.
      then false
      else true
    elif (a * b).Value < 0
    then true
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

  let getVariable k =
    variableManager.get k

  let setVariable k v =
    if log.IsTraceEnabled then log.Trace $"setVariable: #{k}={v}"
    variableManager.set k v
    stampingEventHandler.SetData ($"#{k}", v)

  let rec setVariables = function
  | [] -> ()
  | ((Number(x) as vx), (Number(y) as vy))::q ->
    begin
      setVariable (intOfFloat x) y
      setVariables q
    end
  | (vx, vy)::q ->
    begin
      log.Error $"setVariables: unsupported {vx}={vy}";
      setVariables q
    end

  let floatOfBool = function
  | true -> 1.
  | false -> 0.
  
  let distanceOfFloat f =
    match modalGroupValues.Get LengthUnitGroup with
    | Some(20.) -> In(f)
    | Some(21.) -> Mm(f)
    | Some(x) -> failwith $"Invalid length unit group value {x}"
    | None -> UnknownUnit(f)

  let feedrateOfFloat x =
    match modalGroupValues.Get LengthUnitGroup with
    | Some(20.) -> IPM (x)
    | Some(21.) -> MmMin (x)
    | Some(x) -> failwith $"Invalid length unit group value {x}"
    | None -> UnknownFeedUnit(x)

  let isIncremental () =
    match modalGroupValues.Get DistanceModeGroup with
    | Some(90.) | None -> false
    | Some(91.) -> true
    | Some(x) -> failwith $"Invalid distance mode group {x}"

  let computeLinearDistance { X = x; Y = y; Z = z } =
    let incremental = isIncremental () in
    let dx = if incremental then x else x <-> position.X in
    let dy = if incremental then y else y <-> position.Y in
    let dz = if incremental then z else z <-> position.Z in
    computeDistanceFromPositions (List.filter (fun (x: AxisPosition) -> x.HasValue) [dx; dy; dz])

  /// Set the angle in (-PI,+PI]
  let rec normalizeAngle a =
    let circleRound = 2. * Math.PI in
    if a < -Math.PI
    then normalizeAngle (circleRound + a)
    elif a >= Math.PI
    then normalizeAngle (a - circleRound)
    else a

  let xor a b = (a || b) && not(a && b)
  
  let roundCosinus = function
  | x when (abs x) <= 1. -> x
  | x when (1. < x) && (x < 1.01) -> 1.
  | x when  (-1.01 < x) && (x < -1.) -> -1.
  | x -> x

  let computeArcDistanceWithRadiusCenter p clockwise r cc =
    let v1 = Vector(cc,position) in
    let v2 = Vector(cc,p) in
    let scalarProduct = v1 * v2 in
    let cosangle = roundCosinus (scalarProduct / (r * r)) in
    if (abs cosangle) <= 1.
    then
      let mutable angle = acos cosangle in
      let axy1 = atan (v1.Y / v1.X) in
      let axy2 = atan (v2.Y / v2.X) in
      let axyd = normalizeAngle (axy2 - axy1) in
      if xor (axyd < 0) clockwise
      then angle <- 2. * Math.PI - angle
      r .*. angle
    else 
      log.Error $"computeArcDistanceWithRadius: {cosangle} is not a valid cosinus pos1={position} pos2={p} cc={cc} => return 0."; UnknownUnit(0.)

  let computeArcDistanceWithCenter p clockwise cc =
    match computeLinearDistance cc with
    | In(0.) | Mm(0.) | UnknownUnit(0.) -> distanceOfFloat 0.
    | UnknownAxisPosition -> distanceOfFloat 0.
    | r -> computeArcDistanceWithRadiusCenter p clockwise r cc

  let computeArcDistanceWithRadius p (r: AxisPosition) =
    let ab = computeLinearDistance p in // distance between the start end and the end
    if (ab > r.Map (abs) .*. 2.)
    then log.Error $"computeArcDistanceWithRadius: the distance between start={position} and {p} is greater than 2 r={r}"; UnknownUnit(0.)
    else
      let mutable alpha = 2. * asin (ab / r.Map (abs) / 2.) in
      if r < UnknownUnit(0.) then alpha <- 2. * Math.PI - alpha
      r .*. alpha

  let computeArcDistance p clockwise =
    // TODO: helecoidal
    let computeCircleCenter i j k =
      if configuration.IjkAbsolute
      then { unknownCoordinates with X = i; Y = j; Z = k }
      else { unknownCoordinates with X = i+position.X; Y = j+position.Y; Z = k+position.Z }
    in
    match p with
    | { I = i; J = j; K = k; R = r } when (!?i || !?j || !?j) && (configuration.IjkPrecedence || not !?r) ->
      let cc = computeCircleCenter i j k in
      computeArcDistanceWithCenter p clockwise cc
    | { R = UnknownAxisPosition } -> log.Error $"computeArcDistance: missing I, J, K or R information"; UnknownUnit(0.)
    | { R = r } -> computeArcDistanceWithRadius p r

  let computeDistance p =
    match modalGroupValues.Get MotionGroup with
    | Some(1.) | Some(0.) -> computeLinearDistance p
    | Some(2.) -> computeArcDistance p true
    | Some(3.) -> computeArcDistance p false
    | Some(4.) -> distanceOfFloat 0.
    | Some(x) -> log.Error $"computeDistance: not supported motion group {x}"; computeLinearDistance p
    | None -> failwith "no motion group is defined in computeDistance"
  
  let computeTimeFeedratePerMinute d f =
    match (d, f) with
    | (Mm(dmm), MmMin(fmm)) -> TimeSpan.FromMinutes (dmm / fmm)
    | (In(din), IPM(fipm)) -> TimeSpan.FromMinutes (din / fipm)
    | (UnknownUnit(du), UnknownFeedUnit(fu)) -> TimeSpan.FromMinutes (du / fu)
    | (Mm(dmm), IPM(fipm)) -> TimeSpan.FromMinutes ((inOfMm dmm) / fipm)
    | (In(din), MmMin(fmm)) -> TimeSpan.FromMinutes ((mmOfIn din) / fmm)
    | (In(dx), UnknownFeedUnit(fu)) | (Mm(dx), UnknownFeedUnit(fu))->
      begin
        if log.IsTraceEnabled then log.Trace $"computeTimeFeedratePerMinuteLinear: unknown feed unit, d={dx} f={fu}"
        TimeSpan.FromMinutes (dx / fu)
      end
    | (UnknownUnit(du), MmMin(fx)) | (UnknownUnit(du), IPM(fx)) ->
      begin
        if log.IsTraceEnabled then log.Trace $"computeTimeFeedratePerMinuteLinear: unknown distance unit, d={du} f={fx}"
        TimeSpan.FromMinutes (du / fx)
      end
    | _ ->
      begin
        log.Error $"computeTimeFeedratePerMinuteLinear: Invalid distance {d} feedrate {f}"
        TimeSpan.FromMinutes (0.)
      end
 
  let computeMachiningDistanceTime pos =
    let f =
      match modalGroupValues.Get FGroup with
      | Some(x) -> x
      | None -> failwith "no feedrate is defined in computeMachiningTime"
    in
    let d = computeDistance pos in
    match (modalGroupValues.Get FeedRateModeGroup) with
    | Some(93.) -> (d, TimeSpan.FromMinutes (1. / f))
    | Some(94.) | None -> (d, computeTimeFeedratePerMinute d (feedrateOfFloat f))
    | Some(95.) ->
      match modalGroupValues.Get SGroup with
      | Some(s) -> (d, computeTimeFeedratePerMinute d (feedrateOfFloat (f * s)))
      | None -> failwith "no spindle speed is defined in computeMachiningTime"
    | Some(x) ->
      begin
        log.Error $"computeMachiningTime: invalid feed rate mode group {x}"
        raise (InvalidGroupValueException (FeedRateModeGroup, x))
      end
  
  let computeTimeFromVelocity d u (v: Nullable<float>) =
    if v.HasValue = false then 
      log.Warn "computeTimeFromVelocity: the velocity is null => return 0s"; TimeSpan.FromSeconds (0.)
    else
      match (d, u) with
      | (Mm(0.),_) | (In(0.),_) | (UnknownUnit(0.),_) -> TimeSpan.FromSeconds (0.)
      | (Mm(dmm),_) | (UnknownUnit(dmm),AxisUnit.Mm)-> TimeSpan.FromSeconds (dmm / v.Value)
      | (In(din),_) | (UnknownUnit(din),AxisUnit.In) -> TimeSpan.FromSeconds ((mmOfIn din) / v.Value)
      | (UnknownUnit(dd),_) ->
        begin
          log.Debug "computeTimeFromVelocity: distance with an unknown unit consider it as mm"
          TimeSpan.FromSeconds (dd / v.Value)
        end
      | (UnknownAxisPosition,_) ->
        begin
          log.Debug "computeTimeFromVelocity: unknown distance, skip it => return 0"
          TimeSpan.FromSeconds (0.)
        end

  let computeRapidTraverseDistanceTime { X = x; Y = y; Z = z } =
    let mutable d = UnknownAxisPosition in
    let mutable t = TimeSpan.FromSeconds(0.) in
    let incremental = isIncremental () in
    let dx = if incremental then x.Map (abs) else x <-> position.X in
    let ux = axisPropertyGetter.GetDefaultUnit ("X") in
    let vx = axisPropertyGetter.GetMaxVelocity ("X") in
    let tx = computeTimeFromVelocity dx ux vx in
    d <- dx;
    t <- tx;
    let dy = if incremental then y.Map (abs) else y <-> position.Y in
    let uy = axisPropertyGetter.GetDefaultUnit ("Y") in
    let vy = axisPropertyGetter.GetMaxVelocity ("Y") in
    let ty = computeTimeFromVelocity dy uy vy in
    if (t < ty) || (ty = t && d.HasValue && dy.HasValue && d < dy) then
      begin
        d <- dy
        t <- ty
      end
    let dz = if incremental then z.Map (abs) else z <-> position.Z in
    let uz = axisPropertyGetter.GetDefaultUnit ("Z") in
    let vz = axisPropertyGetter.GetMaxVelocity ("Z") in
    let tz = computeTimeFromVelocity dz uz vz in
    if (t < tz) || (tz = t && d.HasValue && dz.HasValue && d < dz) then
      begin
        d <- dz
        t <- tz
      end
    (d, t)

  let rec getDwellTime = function
    | [] -> log.Error $"getDwellTime: missing parameter"; TimeSpan.FromSeconds(0.)
    | ['P', Number(v)] -> TimeSpan.FromMilliseconds(v)
    | ['P', Undefined(_)] -> log.Warn $"getDwellTime: undefined variable for P parameter => return 0s"; TimeSpan.FromMilliseconds(0.)
    | ['X', Number(v)] | ['U', Number(v)] -> TimeSpan.FromSeconds(v)
    | ['X', Undefined(_)] | ['U', Undefined(_)] -> log.Warn $"getDwellTime: undefined variable for X or U parameter => return 0s"; TimeSpan.FromSeconds(0.)
    | t::q -> getDwellTime q

  let computeDistanceMotionTime pos parameters =
    match modalGroupValues.Get MotionGroup with
    | Some(1.) | Some(2.) | Some(3.) -> computeMachiningDistanceTime pos
    | Some(0.) -> computeRapidTraverseDistanceTime pos
    | Some(4.) -> (distanceOfFloat 0., getDwellTime parameters)
    | _ -> (distanceOfFloat 0., TimeSpan.FromSeconds(0.))

  let isMachining () =
    match modalGroupValues.Get MotionGroup with
    | Some(0.) | Some(4.) -> false
    | None -> false
    | _ -> true

  let getSecondaryHomePosition = function
  | Number(1.) -> { unknownCoordinates with X = distanceOfFloat 0.; Y = distanceOfFloat 0.; Z = distanceOfFloat 0. } // Not sure for I, J, K, R
  | Number(_) -> { unknownCoordinates with X = distanceOfFloat 0.; Y = distanceOfFloat 0.; Z = distanceOfFloat 0. } // Not sure for I, J, K, R (* TODO: ...*)
  | Undefined(s) -> raise (UndefinedPositionException s)

  let callPath path e p =
    if log.IsTraceEnabled then log.Trace $"callPath: path={path} edit={e} parameters={p}"
    edit <- e::edit
    callLevel <- callLevel + 1
    variableManager.startLocalLevel ()
    variableManager.pushLocalParameters p
    try
      subProgramParser.ParseFile path
    with
      | ex ->
        log.Debug($"callPath: exception", ex)
        stampingEventHandler.SetData("PathParameter", List.fold (fun x (c,v) -> $"{x}{c}={v} ") "" p)
    variableManager.endLocalLevel ()
    callLevel <- callLevel - 1
    edit <- edit.Tail
    stampingEventHandler.ResumeProgram(edit.Head, callLevel)

  let callMacro n e p =
    if log.IsTraceEnabled then log.Trace $"callMacro: n={n} edit={e} parameters={p}"
    try
      let path =
        match configuration.SubProgramDirectory with
        | None | Some ("") -> failwith "No sub-program directory is defined"
        | Some (d) ->
          let path1 = System.IO.Path.Combine(d, $"o{n}") in
          if System.IO.File.Exists(path1) then path1
          else
            let path2 = System.IO.Path.Combine(d, $"{n}.txt") in
            if System.IO.File.Exists(path2) then path2
            else raise (System.IO.FileNotFoundException(null, path2))
        in
      callPath path e p
    with
      | ex ->
        log.Debug($"callMacro: exception", ex)
        stampingEventHandler.SetData("Macro", n)

  let callFile f e p =
    if log.IsTraceEnabled then log.Trace $"callFile: f={f} edit={e} parameters={p}"
    try
      let path =
        match configuration.SubProgramDirectory with
        | None | Some ("") -> failwith "No sub-program directory is defined"
        | Some (d) ->
          let path1 = System.IO.Path.Combine(d, f) in
          if System.IO.File.Exists(path1) then path1
          else
            let path2 = System.IO.Path.Combine(d, $"f.txt") in
            if System.IO.File.Exists(path2) then path2
            else raise (System.IO.FileNotFoundException(null, path2))
        in
      callPath path e p
    with
      | ex ->
        log.Debug($"callMacro: exception", ex)
        stampingEventHandler.SetData("SubProgram", f)

  let runGCode (g, x, p) other (file: string option) =
    if log.IsTraceEnabled then log.Trace $"runGCode: g={g} x={x} p={p} other codes={other}"
    let group = gcodeToGroup configuration.Variant (g, Number(x)) in
    begin
      if isGCodeGroupModal group then modalGroupValues.Set group x
      match group with
      | MotionGroup when x = 0. -> stampingEventHandler.SetData("G-Motion", x)
      | MotionGroup when x <> 0. ->
        begin
          stampingEventHandler.SetData("G-Motion", x)
          stampingEventHandler.TriggerMachining()
        end
      | LengthUnitGroup when x = 20. -> setUnitLength LengthIn
      | LengthUnitGroup when x = 21. -> setUnitLength LengthMm
      | LengthUnitGroup -> log.Error $"runGCode: unexpected length unit {x}"
      | ReferenceLocationGroup ->
        begin
          stampingEventHandler.SetData("G-ReferenceLocation", x)
          match (extractPositionFromParameters p position distanceOfFloat configuration) with
          | { X = UnknownAxisPosition; Y = UnknownAxisPosition; Z = UnknownAxisPosition } -> ()
          | intermediatePosition ->
            begin
              let (d, t) = computeRapidTraverseDistanceTime intermediatePosition in
              stampingEventHandler.SetMachiningTime t
              addDistance d
              position <- intermediatePosition
            end
          // Note: home position is in machine coordinate and not work coordinate, so it can't be used directly
          let homePosition = 
            match x with
            | 28. -> { unknownCoordinates with X = distanceOfFloat 0.; Y = distanceOfFloat 0.; Z = distanceOfFloat 0. } // Not sure for I, J, K, R
            | 30. -> getSecondaryHomePosition (extractNamedParameter p 'P')
            | _ ->
              begin
                log.Error $"runGCode: invalid reference location group value {x}"
                { unknownCoordinates with X = distanceOfFloat 0.; Y = distanceOfFloat 0.; Z = distanceOfFloat 0. } // Not sure for I, J, K, R
              end
            in
          stampingEventHandler.SetMachiningTime(snd (computeRapidTraverseDistanceTime homePosition)) // Note: this is wrong
          position <- unknownCoordinates // Note: see remark above
        end
      | CoordinateSystemSelectionGroup ->
        begin
          position <- unknownCoordinates
        end
      | ToolChangeGroup ->
        begin
          stampingEventHandler.TriggerToolChange()
          stampingEventHandler.SetMachiningTime(TimeSpan.FromSeconds (configuration.StandardToolChangeTime))
        end
      | TGroup -> stampingEventHandler.SetNextToolNumber (x.ToString ())
      | FGroup -> stampingEventHandler.SetData ("F", x)
      | SGroup -> stampingEventHandler.SetData ("S", x)
      | FeedRateModeGroup -> stampingEventHandler.SetData ("G-FeedRateMode", x)
      | HpccG05Group ->
        begin
          match p with
          | ('P', Number(hpcc))::[] ->
            begin
              stampingEventHandler.SetData ("G05P", hpcc) // 0: inactive, 10000: active
              modalGroupValues.Set group hpcc
              end
          | (pkey, pvalue)::[] -> log.Error $"runGCode: unexpected parameter {pkey}={pvalue} for G05"
          | _ -> log.Error $"runGCode: unexpected list of parameters for G05"
        end
      | AiccAiapcG051Group ->
        begin
          match p with
          | ('Q', Number(a))::_ ->
            begin
              stampingEventHandler.SetData ("G05.1Q", a) // 0: inactive, else active
              modalGroupValues.Set group a
            end
          | (pkey, pvalue)::_ -> log.Error $"runGCode: unexpected first parameter {pkey}={pvalue} for G05.1"
          | _ -> log.Error $"runGCode: unexpected list of parameters for G05.1"
        end
      | AdvancedPreviewControlGroup ->
        begin
          match p with
          | ('P', Number(apc))::[] ->
            begin
              stampingEventHandler.SetData ("G08P", apc) // 0: inactive, else active
              modalGroupValues.Set group apc
            end
          | (pkey, pvalue)::[] -> log.Error $"runGCode: unexpected first parameter {pkey}={pvalue} for G08"
          | _ -> log.Error $"runGCode: unexpected list of parameters for G08"
        end
      | _ -> ()
      match (g, x) with
      | ('M', 0.) -> if not initialization then stampingEventHandler.SuspendProgram()
      | ('M', 1.) -> if not initialization then stampingEventHandler.SuspendProgram(optional = true)
      | ('M', 2.) | ('M', 30.) | ('M', 60.) | ('M', 99.) -> if not initialization then stampingEventHandler.EndProgram (edit.Head, callLevel, false)
      | ('G', 65.) ->
        match extractNamedParameter p 'P' with
        | Number (x) -> callMacro (int x) false (List.filter (fun (c,_) -> c <> 'P') p)
        | Undefined (_) -> failwith (sprintf "Missing parameter P for macro call G65")
      | ('M', 98.) ->
        if file.IsSome
        then
          callFile file.Value false []
        else
          match extractNamedParameter p 'P' with
          | Number (x) -> 
            try
              let l =
                match extractNamedParameter p 'L' with
                | Number (y) -> int y
                | Undefined (_) -> 1
              in
              for i in 1 .. l do
                callMacro (int x) false [] (* false: edit *)
            with
            | UndefinedParameterException _ -> callMacro (int x) false []
          | Undefined (_) -> failwith "Missing parameter P for M98"
      | _ -> ()
    end

  let rec runGCodes l p f =
    match l with
    | [] -> ()
    | h::q -> begin runGCode h p f; runGCodes q p f end

  let runParameter = function
  | (k, v) when (isAxis k) -> stampingEventHandler.SetData (k.ToString (), v) 
  | _ -> ()

  let rec runParameters = function
  | [] -> ()
  | h::q -> begin runParameter h; runParameters q end

  let emptyBlock = { N = None; GCodes = []; Parameters = []; SetVariables = []; Comment = None; StampVariable = false; File = None; Escapes = [] }

  let rec addHeaderComments = function
  | [] -> ()
  | Comment(c)::_ -> stampingData.Add("ProgramComment", c.Trim())
  | _::q -> addHeaderComments q

  let rec addBlockInstructions blockNumber = function
    | [] -> emptyBlock
    | XCode('N', Number(n))::q ->
      begin
        let b = addBlockInstructions blockNumber q in
        { b with N = Some(intOfFloat n) }
      end
    | XCode('N', x)::q ->
      begin
        log.Error $"addBlockInstructions: invalid N code {x}"
        addBlockInstructions blockNumber q
      end
    | Comment(x)::q ->
      begin
        let b = addBlockInstructions blockNumber q in
        match b.Comment with
        | None -> { b with Comment = Some(x) }
        | Some c ->
          log.Warn $"addBlockInstructions: new comment {x} while {c} is already set => do nothing"
          b
      end
    | Extra(x)::q -> addBlockInstructions blockNumber q // Note: for the moment, do nothing with extra
    | File(x)::[] -> { emptyBlock with File = Some(x) }
    | File(x)::q ->
      log.Warn "addBlockInstructions: g-code after file"
      let b = addBlockInstructions blockNumber q in
      { b with File = Some(x) }
    | XCode(g, Number(x))::q when (isGCodeByDefault g) -> processGCodeWithParameters blockNumber g x [] q 1
    | XCode(g, Number(x))::q -> addParameter (addBlockInstructions blockNumber q) g (Number x)
    | XCode(g, Undefined(s))::q ->
      begin
        log.Error $"addBlockInstructions: skip {g}{s} because of undefined variables"
        addBlockInstructions blockNumber q
      end
    | SetVariable(Number(x), Number(y))::q when stampVariablesGetter.IsStampVariable ((intOfFloat x).ToString()) ->
      begin
        if log.IsTraceEnabled then log.Trace $"addBlockInstructions: StampVariable {x} detected"
        let b = addBlockInstructions blockNumber q in
        { b with StampVariable = true }
      end
    | SetVariable(vx,vy)::q ->
      begin
        let b = addBlockInstructions blockNumber q in
        { b with SetVariables = (vx,vy)::b.SetVariables }
      end
    | Escape(n)::q ->
        log.Info $"addBlockInstructions: escape {n}"
        let b = addBlockInstructions blockNumber q in
        { b with Escapes = n::b.Escapes }
  and processGCodeWithParameters blockNumber g x p q i =
    if log.IsTraceEnabled then log.Trace $"processGCodeWithParameters: g={g} x={x} p={p} q={q} i={i}"
    match q with
    | [] -> addGCode configuration.Variant (addBlockInstructions blockNumber []) g x p
    | XCode(a, v)::q when not (isParameterOfGCode g x a i) -> addGCode configuration.Variant (addBlockInstructions blockNumber (XCode(a, v)::q)) g x p
    | XCode(a, v)::q -> processGCodeWithParameters blockNumber g x (p@[(a,v)]) q (i+1)
    | r -> addGCode configuration.Variant (addBlockInstructions blockNumber r) g x p

  let addBlockInstructionsOrHeader blockNumber = function
    | XCode('O', Number(ocode))::comments ->
      begin
        if not headerSection then
          log.Error $"addBlockInstructionsOrHeader: ocode={ocode} is not on the first blocks, block #={blockNumber}"
        elif not initialization then
          if log.IsDebugEnabled then log.Debug $"addBlockInstructions: ocode={ocode} comments={comments}"
          stampingEventHandler.StartProgram (edit.Head, callLevel)
        addHeaderComments comments
        emptyBlock
      end
    | Comment(x)::[] as comments ->
      begin
        if headerSection then
          addHeaderComments comments
          emptyBlock
        else
          addBlockInstructions blockNumber comments
      end
    | instructions ->
      begin
        if headerSection then
          if not initialization then
            if log.IsDebugEnabled then log.Debug $"addBlockInstructionsOrHeader: no ocode"
            stampingEventHandler.StartProgram (edit.Head, callLevel)
          headerSection <- false
        addBlockInstructions blockNumber instructions
      end

  let operatorFromString = function
  | "**" -> fun x y -> Math.Pow (x, y)
  | "*" -> (*)
  | "/" -> (/)
  | "MOD" -> (%)
  | "+" -> (+)
  | "-" -> (-)
  | "EQ" -> fun x y -> floatOfBool (x = y)
  | "NE" -> fun x y -> floatOfBool (x <> y)
  | "GT" -> fun x y -> floatOfBool (x > y)
  | "GE" -> fun x y -> floatOfBool (x >= y)
  | "LT" -> fun x y -> floatOfBool (x < y)
  | "LE" -> fun x y -> floatOfBool (x <= y)
  | "AND" -> fun x y -> floatOfBool ((x <> 0.) && (y <> 0.))
  | "OR" -> fun x y -> floatOfBool ((x <> 0.) || (y <> 0.))
  | "XOR" -> fun x y -> floatOfBool (if (x <> 0.) then (y = 0.) else (y <> 0.))
  | op -> failwith (sprintf "Unsupported operation %s" op)

  let functionFromString = function
  | "ATAN" -> Math.Atan
  | "ABS" -> Math.Abs
  | "ACOS" -> Math.Acos
  | "ASIN" -> Math.Asin
  | "COS" -> Math.Cos
  | "EXP" -> Math.Exp
  | "FIX" -> Math.Floor
  | "FUP" -> Math.Ceiling
  | "ROUND" -> Math.Round
  | "LN" -> Math.Log
  | "SIN" -> Math.Sin
  | "TAN" -> Math.Tan
  | f -> failwith (sprintf "Unsupported function %s" f)

  member this.StartInitialization () =
    initialization <- true

  member this.EndInitialization () =
    initialization <- false

  member this.InitializeGCode (g, x) =
    let group = gcodeToGroup configuration.Variant (g, Number(x)) in
    if isGCodeGroupModal group then
      modalGroupValues.Set group x
    else
      failwith $"Group {g} is not modal"
    match group with
    | LengthUnitGroup when x = 20. -> setUnitLength LengthIn
    | LengthUnitGroup when x = 21. -> setUnitLength LengthMm
    | LengthUnitGroup -> log.Error $"InitializeGCode: unexpected length unit {x}"
    | _ -> ()

  member this.InitializeGCodes ([<ParamArray>] args: (char*float)[]) =
    for arg in args do this.InitializeGCode arg
  
  member this.InitializeVariable (k, x) =
    setVariable k x

  member this.InitializeVariables ([<ParamArray>] args: (int*float)[]) =
    for arg in args do this.InitializeVariable arg

  member this.AddHeaderComments comments =
    if not initialization then addHeaderComments comments
    ()

  member this.AddBlock instructions =
    begin
      if log.IsTraceEnabled then log.Trace $"AddBlock: instructions={instructions}"
      blockNumber <- blockNumber + 1
      let block = addBlockInstructionsOrHeader blockNumber instructions in
      if List.isEmpty block.Escapes then
      match block.Comment with
      | None -> ()
      | Some c -> stampingEventHandler.SetComment (c)
        setVariables block.SetVariables
      let gcodes = block.GCodes in
      let parameters = block.Parameters in
      runGCodes gcodes parameters block.File
      match gcodes with
      | _::_ when configuration.AnyGCodeMachining -> stampingEventHandler.TriggerMachining ()
      | _ -> ()
      let pos = extractPositionFromParameters parameters position distanceOfFloat configuration in
      let (distance, motionTime) = computeDistanceMotionTime pos parameters in
      if log.IsTraceEnabled then log.Trace $"AddBlock: d={distance} t={motionTime} for {instructions}"
      if 0. <> motionTime.TotalSeconds then
        if (isMachining () && not configuration.AnyGCodeMachining) then stampingEventHandler.TriggerMachining ()
        stampingEventHandler.SetMachiningTime (motionTime)
      if (position <> pos) || (distance.HasValue && (0. < distance.Value)) then
        addDistance distance
        let newVector = Vector (position, pos) in
        if vector.IsSome then recordNewVector newVector
        position <- pos
        if newVector.Empty then log.Error $"AddBlock: newVector {newVector} is empty"
        vector <- Some newVector
      block
    end

  member this.NotifyNewBlock block pos =
    if not initialization then
      if callLevel = 0
      then
        if block.StampVariable
        then ncProgramReaderWriter.Skip (pos)
        else ncProgramReaderWriter.Release (pos)
      stampingEventHandler.NotifyNewBlock (edit.Head, callLevel)

  member this.NotifyEndOfFile pos =
    if not initialization then
      if callLevel = 0 then
        stampingEventHandler.EndProgram (edit.Head, callLevel, true)
        ncProgramReaderWriter.Release ()

  member this.SetData key v =
    stampingEventHandler.SetData (key, v)

  member this.ResolveVariable k =
    match k with 
    | Number(x) ->
      try
        let r = Number(getVariable (intOfFloat x)) in
        if log.IsTraceEnabled then log.Trace $"ResolveVariable: #{k}={r}"
        r
      with
      | UnknownVariableException(k) -> 
        begin
          if log.IsDebugEnabled then log.Debug $"ResolveVariable: unknown variable {k}"
          Undefined(Set.empty.Add(k))
        end
    | Undefined(s) -> Undefined(s) 

  member this.ApplyOperator x op y =
    if log.IsTraceEnabled then log.Trace $"ApplyOperator: {x} {op} {y}"
    match (x, y) with
    | (Number(a),Number(b)) -> Number(a |> (operatorFromString op) <| b)
    | (Number(_),Undefined(s)) -> Undefined(s)
    | (Undefined(s),Number(_)) -> Undefined(s)
    | (Undefined(a),Undefined(b)) -> Undefined(Set.union a b)

  member this.ApplyFunction f x =
    match x with
    | Number(a) -> Number((functionFromString f) <| a)
    | Undefined(s) -> Undefined(s)
