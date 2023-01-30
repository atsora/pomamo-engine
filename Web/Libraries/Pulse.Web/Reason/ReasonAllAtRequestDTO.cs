// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Reason/AllAt service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Reason/AllAt/", "GET", Summary = "Get all the reasons at the specified UTC date/time", Notes = "To use with ?MachineId=&At=")]
  [Route ("/Reason/AllAt/Get", "GET", Summary = "Get all the reasons at the specified UTC date/time", Notes = "To use with ?MachineId=&At=")]
  [Route ("/Reason/AllAt/Get/{MachineId}/{At}", "GET", Summary = "Get all the reasons at the specified UTC date/time", Notes = "")]
  [Route ("/ReasonAllAt/", "GET", Summary = "Get the reason (only) slots in a specified range", Notes = "To use with ?MachineId=&At=")]
  public class ReasonAllAtRequestDTO : IReturn<ReasonAllAtResponseDTO>
  {
    /// <summary>
    /// Id of the monitored machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// UTC date/time of the request
    /// </summary>
    [ApiMember (Name = "At", Description = "UTC date/time of the request in ISO format", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string At { get; set; }
  }
}
