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
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Cache
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Cache/ClearDomainByMachineModule service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Cache/ClearDomainByMachineModule", "GET", Summary = "Clear a domain cache by MachineModule", Notes = "To be used with ?Domain=&MachineModuleId=&Broadcast=")]
  [Route("/Cache/ClearDomainByMachineModule/{Domain}/{MachineModuleId}", "GET", Summary = "Clear a domain cache by MachineModule", Notes = "")]
  [Route("/Cache/ClearDomainByMachineModule/Get/{Domain}/{MachineModuleId}", "GET", Summary = "Clear a domain cache by MachineModule", Notes = "")]
  [AllowAnonymous]
  public class ClearDomainByMachineModuleRequestDTO: IReturn<OkDTO>
  {
    /// <summary>
    /// Domain name
    /// </summary>
    [ApiMember(Name="Domain", Description="Domain name", ParameterType="path", DataType="string", IsRequired=true)]
    public string Domain { get; set; }    
    
    /// <summary>
    /// MachineModule ID
    /// </summary>
    [ApiMember(Name="MachineModuleId", Description="MachineModule Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineModuleId { get; set; }

    /// <summary>
    /// Broadcast to all the other web service
    /// </summary>
    [ApiMember (Name = "Broadcast", Description = "Broadcast to all the other web services", ParameterType = "path", DataType = "bool", IsRequired = false)]
    public bool Broadcast { get; set; } = true;

    /// <summary>
    /// Wait for completion before returning the response
    /// </summary>
    [ApiMember (Name = "Wait", Description = "Wait for completion before returning the response", ParameterType = "path", DataType = "bool", IsRequired = false)]
    public bool Wait { get; set; } = false;
  }
}

