(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.Stamping.Heidenhain

open System

type Configuration () =

  /// Standard tool change time in seconds
  member val StandardToolChangeTime = 5. with get, set
  
  /// Minimum angle in degrees to record / log
  member val MinRecordedAngle = 30 with get, set
