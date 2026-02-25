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
  [Api("Request DTO for /Cache/ClearDomainByMachine service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Cache/ClearDomainByMachine", "GET", Summary = "Clear a domain cache by machine", Notes = "To be used with ?Domain=&MachineId=&Broadcast=")]
  [Route("/Cache/ClearDomainByMachine/{Domain}/{MachineId}", "GET", Summary = "Clear a domain cache by machine", Notes = "")]
  [Route("/Cache/ClearDomainByMachine/Get/{Domain}/{MachineId}", "GET", Summary = "Clear a domain cache by machine", Notes = "")]
  [AllowAnonymous]
  public class ClearDomainByMachineRequestDTO: IReturn<OkDTO>
  {
    /// <summary>
    /// Domain name
    /// </summary>
    [ApiMember(Name="Domain", Description="Domain name", ParameterType="path", DataType="string", IsRequired=true)]
    public string Domain { get; set; }    
    
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Broadcast to all the other web service
    /// </summary>
    [ApiMember (Name = "Broadcast", Description = "Broadcast to all the other web services", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool Broadcast { get; set; } = true;

    /// <summary>
    /// Wait for completion before returning the response
    /// </summary>
    [ApiMember (Name = "Wait", Description = "Wait for completion before returning the response", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool Wait { get; set; } = false;
  }
}

