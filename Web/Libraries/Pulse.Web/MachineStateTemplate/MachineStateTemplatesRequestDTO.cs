// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.MachineStateTemplate
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /MachineStateTemplates service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/MachineStateTemplates/", "GET", Summary = "Get the list of possible machine state templates", Notes = "To use with ?RoleId=")]
  [Route("/MachineStateTemplates/Get/{RoleId}", "GET", Summary = "Get the list of possible machine state templates for a specific role", Notes = "")]
  [Route("/MachineStateTemplate/MachineStateTemplates/", "GET", Summary = "Get the list of possible machine state templates", Notes = "To use with ?RoleId=")]
  [Route("/MachineStateTemplate/MachineStateTemplates/Get/{RoleId}", "GET", Summary = "Get the list of possible machine state templates for a specific role", Notes = "")]
  public class MachineStateTemplatesRequestDTO: IReturn<MachineStateTemplatesResponseDTO>
  {
    /// <summary>
    /// Id of the role that requests this next machine state template id
    /// 
    /// Default: 0 (not specified)
    /// </summary>
    [ApiMember(Name="RoleId", Description="Role Id of the requester", ParameterType="path", DataType="int", IsRequired=false)]
    public int RoleId { get; set; }
  }
}
