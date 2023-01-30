(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

/// <summary>
/// This is only done so that the plugin is upgraded and the tables are created by Lem_PackageManager.Console -c
/// before giving the rights to the reports
/// </summary>
type InstallationExtension() =
  interface Lemoine.Extensions.IExtension with
    member this.UniqueInstance = true

  interface Lemoine.Extensions.Business.Config.IInstallationExtension with
    member this.Priority = 0.
    member this.CheckConfig () = true
    member this.RemoveConfig () = true
