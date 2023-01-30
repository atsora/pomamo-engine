(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Model
open System
open Lemoine.Collections

type Milestone(machine, dateTime, message) =
  abstract Id: int with get, set
  default val Id = 0 with get, set

  abstract Version: int with get, set
  default val Version = 0 with get, set

  abstract Machine: IMachine with get, set
  default val Machine = machine with get, set

  abstract DateTime: DateTime with get, set
  default val DateTime = dateTime with get, set

  abstract Message: string with get, set
  default val Message = message with get, set

  abstract CreationDateTime: DateTime with get, set
  default val CreationDateTime = DateTime.UtcNow with get, set

  new() = Milestone (null, DateTime.UtcNow, "")

  interface IDataWithId with
    member this.Id = this.Id
  interface IDataWithVersion with
    member this.Version = this.Version
  interface Lemoine.Model.IPartitionedByMachine with
    member this.Machine = this.Machine
