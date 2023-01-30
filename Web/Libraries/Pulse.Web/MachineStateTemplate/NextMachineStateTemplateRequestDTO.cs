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
  [Api("Request DTO for /NextMachineStateTemplate service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/NextMachineStateTemplate/", "GET", Summary = "Get the next possible machine state templates to a specified machine state template", Notes = "To use with ?CurrentMachineStateTemplateId=&RoleId=")]
  [Route("/NextMachineStateTemplate/Get/{CurrentMachineStateTemplateId}", "GET", Summary = "Get the next possible machine state templates to a specified machine state template", Notes = "")]
  [Route("/MachineStateTemplate/NextMachineStateTemplate/", "GET", Summary = "Get the next possible machine state templates to a specified machine state template", Notes = "To use with ?MachineStateTemplateId=&RoleId=")]
  [Route("/MachineStateTemplate/NextMachineStateTemplate/Get/{CurrentMachineStateTemplateId}", "GET", Summary = "Get the next possible machine state templates to a specified machine state template", Notes = "")]
  public class NextMachineStateTemplateRequestDTO: IReturn<NextMachineStateTemplateResponseDTO>
  {
    /// <summary>
    /// Id of the current machine state template
    /// 
    /// If 0 or not set, it is considered there is currently no active machine state template,
    /// and all the machine state templates in the database may be returned
    /// </summary>
    [ApiMember(Name="CurrentMachineStateTemplateId", Description="Current machine state template Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int CurrentMachineStateTemplateId { get; set; }
    
    /// <summary>
    /// Id of the role that requests this next machine state template id
    /// 
    /// Default: 0 (not specified)
    /// </summary>
    [ApiMember(Name="RoleId", Description="Role Id of the requester", ParameterType="path", DataType="int", IsRequired=false)]
    public int RoleId { get; set; }
  }
}
