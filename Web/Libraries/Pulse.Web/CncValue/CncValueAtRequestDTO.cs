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

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /CncValueAt service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/CncValueAt/", "GET", Summary = "Get the cnc values at a specified UTC date/time", Notes = "To use with ?MachineId=&At=")]
  [Route ("/CncValueAt/Get/{MachineId}/{At}", "GET", Summary = "Get the cnc values at a specitified date/time for the specified machine ID")]
  [Route ("/CncValue/At/", "GET", Summary = "Get the cnc values at a specified UTC date/time", Notes = "To use with ?MachineId=&At=")]
  [Route ("/CncValue/At/Get/{MachineId}/{At}", "GET", Summary = "Get the cnc values at a specitified date/time for the specified machine ID")]
  public class CncValueAtRequestDTO : IReturn<CncValueAtResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id in case the main machine module is targetted", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Date/time
    /// </summary>
    [ApiMember (Name = "At", Description = "Requested date/time", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string At { get; set; }
  }
}
