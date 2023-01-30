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

namespace Pulse.Web.Tool
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /ToolLivesByMachine service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ToolLivesByMachine/", "GET", Summary = "Get the tool life status for a specific machine", Notes = "To use with ?MachineId=")]
  [Route("/ToolLivesByMachine/{MachineId}", "GET", Summary = "Get the tool life status for a specific machine", Notes = "")]
  [Route ("/ToolLivesByMachine/Get/{MachineId}", "GET", Summary = "Get the tool life status for a specific machine", Notes = "")]
  [Route ("/Tool/ToolLivesByMachine/", "GET", Summary = "Get the tool life status for a specific machine", Notes = "To use with ?MachineId=")]
  [Route("/Tool/ToolLivesByMachine/{MachineId}", "GET", Summary = "Get the tool life status for a specific machine", Notes = "")]
  [Route ("/Tool/ToolLivesByMachine/Get/{MachineId}", "GET", Summary = "Get the tool life status for a specific machine", Notes = "")]
  public class ToolLivesByMachineRequestDTO: IReturn<System.Collections.Generic.IEnumerable<ToolLivesByMachineResponseDTO>>
  {
    /// <summary>
    /// Id of requested machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Max expiration time in seconds
    /// </summary>
    [ApiMember(Name="MaxExpirationTime", Description="Max returned expiration time in seconds", ParameterType="path", DataType="int", IsRequired=false)]
    public int? MaxExpirationTime { get; set; }
  }
}
