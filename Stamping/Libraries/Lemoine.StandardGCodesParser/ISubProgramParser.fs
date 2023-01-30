(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

type ISubProgramParser =
  abstract ParseFile: string -> unit
