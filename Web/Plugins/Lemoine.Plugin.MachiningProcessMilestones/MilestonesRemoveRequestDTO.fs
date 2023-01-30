(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Extensions.Web.Attributes
open Lemoine.Extensions.Web.Interfaces

#if NSERVICEKIT
[<NServiceKit.ServiceHost.Route ("/Milestones/Remove", "GET", Summary = "Service to remove a new milestone for a specific group id, to use with ?GroupId=&At=&Message=")>]
[<NServiceKit.ServiceHost.Route ("/MilestonesRemove", "GET", Summary = "Service to remove a new milestone for a specific group id, to use with ?GroupId=&At=&Message=")>]
#endif // NSERVICEKIT
[<Api("Request DTO for /Milestones/Remove service")>]
[<ApiResponse(System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")>]
[<Route("/Milestones/Remove", "GET", Summary = "Service to remove a milestone, to use with ?Id=")>]
[<Route("/MilestonesRemove", "GET", Summary = "Service to remove a milestone, to use with ?Id=")>]
type MilestonesRemoveRequestDTO() =
#if NSERVICEKIT
  interface NServiceKit.ServiceHost.IReturn<MilestonesRemoveResponseDTO>
#endif // NSERVICEKIT

  /// <summary>
  /// Id of the milestone
  /// </summary>
  [<ApiMember (Name = "Id", Description = "Milestone Id", ParameterType = "path", DataType = "int", IsRequired = true)>]
  member val Id: int = 0 with get, set

  interface IReturn<MilestonesRemoveResponseDTO>
