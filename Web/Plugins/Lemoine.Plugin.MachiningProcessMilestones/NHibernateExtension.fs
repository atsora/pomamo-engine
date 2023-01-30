(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

type NHibernateExtension() =
  interface Lemoine.Extensions.IExtension with
    member this.UniqueInstance = true

  interface Lemoine.Extensions.Database.INHibernateExtension with
    member this.ContainsMapping () = true
    member this.UpdateConfiguration configuration = ()
