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
open Pulse.Extensions.Web.Responses

[<Api("/Milestones/Get Response DTO")>]
type MilestonesGetResponseDTO(range: UtcDateTimeRange) =
  // TODO: new f# operators for Bound, Nullable, ...
  member val Range = range.ToString(fun bound -> ConvertDTO.DateTimeUtcToIsoString(new Bound<DateTime>(Nullable(bound), BoundType.Upper))) with get, set
  member val Machines: System.Collections.Generic.List<MilestonesGetResponseByMachineDTO> = new System.Collections.Generic.List<MilestonesGetResponseByMachineDTO> () with get, set
  
and MilestonesGetResponseByMachineDTO(machine) =
  inherit MachineDTO(machine)
  member val Milestones: System.Collections.Generic.List<MilestoneDto> = new System.Collections.Generic.List<MilestoneDto> () with get, set

and MilestoneDto(id, dateTime: DateTime, message) =
  member val Id: int = id with get, set
  member val DateTime: string = ConvertDTO.DateTimeUtcToIsoString (dateTime) with get, set
  member val Message: string = message with get, set
