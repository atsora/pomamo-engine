(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Extensions.Web.Attributes
open Lemoine.Extensions.Web.Responses
open Lemoine.Model
open System
open Lemoine.FSharp.Model.Bound

[<Api("/Milestones/Save Response DTO")>]
type MilestonesSaveResponseDTO(id) =
  inherit Lemoine.Extensions.Web.Responses.OkDTO("Ok")
  member val id: int = id with get, set
