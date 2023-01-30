(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.StandardGCodesParser

open System.Collections.Generic
open GCode

type ModalGroupValues () =

  let values = new Dictionary<GCodeGroup, float> ()

  member this.Get g =
    let v = ref 0. in
    if values.TryGetValue (g, v) then Some v.Value else None

  member this.Set g x =
    values.Item (g) <- x
