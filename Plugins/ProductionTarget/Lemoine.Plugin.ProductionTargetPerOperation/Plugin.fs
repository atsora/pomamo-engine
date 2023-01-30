(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.ProductionTargetPerOperation

open Lemoine.Core.Log
open Lemoine.Extensions.Plugin
open Lemoine.Model
open System.Data
open Lemoine.Extensions.Interfaces
open Pulse.Extensions.Plugin

type Plugin() =
  inherit PluginWithAutoConfig<Configuration>()
  
  let log = LogManager.GetLogger "Lemoine.Plugin.ProductionTargetPerOperation.Plugin"

  override this.Name = "Production target per operation"
  override this.Description = "Production target per operation"
  override this.Version = 1
  override this.Uninstall () = ()

  override this.InstallVersion v =
    match v with
    | 0 -> ()
    | x -> this.InstallVersion (x-1)

  interface IPluginDll with
     override this.Name = this.Name
     override this.Description = this.Description
     override this.Version = this.Version
     override this.Uninstall () = this.Uninstall ()

  interface IFlaggedPlugin with
     override this.Flags = PluginFlag.Web
