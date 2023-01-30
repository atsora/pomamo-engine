(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

#if !NSERVICEKIT
(* Note: this does not work currently with the web service, see story #175262470 *)

type WebExtension() =

  member val UniqueInstance = true
  member this.GetAssembly () =
    this.GetType().Assembly

  interface Lemoine.Extensions.IExtension with
    member this.UniqueInstance = this.UniqueInstance

  interface Lemoine.Extensions.Web.IWebExtension with
    member this.GetAssembly () = this.GetAssembly ()

#endif // !NSERVICEKIT
