(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
* Copyright (C) 2024 Atsora Solutions
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open System

type Configuration () =

  /// G-code variant: Fanuc, Hurco
  member val Variant = "Fanuc" with get, set

  /// Do U, V or W commands correspond to incremental motion commands?
  member val UvwIncremental = false with get, set

  /// Do I, J or K are absolute or incremental (compare to X, Y and Z)? Default is incremental
  member val IjkAbsolute = false with get, set

  /// Are the I, J and K value modal?
  member val IjkModal = false with get, set

  /// Is R modal?
  member val RModal = false with get, set

  /// Give I, J and K precedence over R
  member val IjkPrecedence = false with get, set

  /// Allow Helical interpretation for G02 and G03
  member val HelicalInterpretation = false with get, set

  /// Does any g-code trigger some machining action?
  member val AnyGCodeMachining = false with get, set

  /// Should G65, M98 and M198 be considered as a machining macro when the macro is unknown?
  member val UnknownMacroMachining = true with get, set

  // TODO: define macros that should be considered as a machining macro

  /// Standard tool change time in seconds
  member val StandardToolChangeTime = 5. with get, set

  /// Minimum angle in degrees to record / log
  member val MinRecordedAngle = 30 with get, set

  /// Sub-program directory
  member val SubProgramDirectory: string option = None with get, set

  /// G-Codes to set before parsing the program. For example: G20G90G94
  member val InitializationGCodes: string option = None with get, set
