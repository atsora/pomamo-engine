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
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.OperationCycle
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /OperationCycle/At service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/OperationCycle/At/", "GET", Summary = "Get the operation cycle at the specified UTC date/time", Notes = "To use with ?MachineId=&At=")]
  [Route ("/OperationCycle/At/Get/{MachineId}/{At}", "GET", Summary = "Get the operation cycle at the specified UTC date/time")]
  [Route ("/OperationCycleAt/", "GET", Summary = "Get the operation cycle at the specified UTC date/time", Notes = "To use with ?MachineId=&At=")]
  public class OperationCycleAtRequestDTO : IReturn<OperationCycleAtResponseDTO>
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
