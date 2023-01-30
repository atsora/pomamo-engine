(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Extensions.Web.Attributes
open Lemoine.Extensions.Web.Interfaces

#if NSERVICEKIT
[<NServiceKit.ServiceHost.Route ("/Milestones/Save", "GET", Summary = "Service to save a new milestone for a specific group id, to use with ?GroupId=&At=&Message=")>]
[<NServiceKit.ServiceHost.Route ("/MilestonesSave", "GET", Summary = "Service to save a new milestone for a specific group id, to use with ?GroupId=&At=&Message=")>]
#endif // NSERVICEKIT
[<Api("Request DTO for /Milestones/Save service")>]
[<ApiResponse(System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")>]
[<Route("/Milestones/Save", "GET", Summary = "Service to save a new milestone for a specific group id, to use with ?GroupId=&At=&Message=")>]
[<Route("/MilestonesSave", "GET", Summary = "Service to save a new milestone for a specific group id, to use with ?GroupId=&At=&Message=")>]
type MilestonesSaveRequestDTO() =
#if NSERVICEKIT
  interface NServiceKit.ServiceHost.IReturn<MilestonesSaveResponseDTO>
#endif // NSERVICEKIT

  /// <summary>
  /// Id of the group
  ///
  /// If not set, all the machines are considered
  /// </summary>
  [<ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)>]
  member val GroupId: string = "" with get, set

  /// <summary>
  /// Date/time of the milestone
  /// </summary>
  [<ApiMember (Name = "At", Description = "Date/time of the milestone", ParameterType = "path", DataType = "string", IsRequired = true)>]
  member val At: string = "" with get, set

  /// <summary>
  /// Date/time of the milestone
  /// </summary>
  [<ApiMember (Name = "Message", Description = "Message", ParameterType = "path", DataType = "string", IsRequired = true)>]
  member val Message: string = "" with get, set

  interface IReturn<MilestonesSaveResponseDTO>
