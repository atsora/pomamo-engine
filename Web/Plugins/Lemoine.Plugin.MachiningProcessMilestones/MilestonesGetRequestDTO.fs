(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Extensions.Web.Attributes
open Lemoine.Extensions.Web.Interfaces

#if NSERVICEKIT
[<NServiceKit.ServiceHost.Route ("/Milestones/Get", "GET", Summary = "Service to get the milestones for a specific group id (if specified)")>]
[<NServiceKit.ServiceHost.Route ("/MilestonesGet", "GET", Summary = "Service to get the milestones for a specific group id (if specified)")>]
#endif // NSERVICEKIT
[<Api("Request DTO for /Milestones/Get service")>]
[<ApiResponse(System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")>]
[<Route("/Milestones/Get", "GET", Summary = "Service to get the milestones for a specific group id (if specified)")>]
[<Route("/MilestonesGet", "GET", Summary = "Service to get the milestones for a specific group id (if specified)")>]
type MilestonesGetRequestDTO() =
#if NSERVICEKIT
  interface NServiceKit.ServiceHost.IReturn<MilestonesGetResponseDTO>
#endif // NSERVICEKIT

  /// <summary>
  /// Id of the group
  ///
  /// If not set, all the machines are considered
  /// </summary>
  [<ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = false)>]
  member val GroupId: string = "" with get, set

  /// <summary>
  /// Requested range
  ///
  /// Default is last 3 months
  /// </summary>
  [<ApiMember (Name = "Range", Description = "Range", ParameterType = "path", DataType = "string", IsRequired = false)>]
  member val Range: string = "" with get, set

  interface IReturn<MilestonesGetResponseDTO>
