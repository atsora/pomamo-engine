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

namespace Pulse.Web.IsoFile
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /IsoFile/Current service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/IsoFile/Current/", "GET", Summary = "", Notes = "To use with ?MachineId=")]
  [Route ("/IsoFile/Current/Get/{MachineId}", "GET", Summary = "", Notes = "")]
  public class IsoFileCurrentRequestDTO : IReturn<IsoFileCurrentResponseDTO>
  {
    /// <summary>
    /// Machine Id
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Include the detection status in the response (default is false)
    /// </summary>
    [ApiMember (Name = "IncludeDetectionStatus", Description = "Include the detection status in the response", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool IncludeDetectionStatus { get; set; } = false;
  }
}
