// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

using System.Net;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Cache
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Cache/ClearPulseInfo service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Cache/ClearPulseInfo", "GET", Summary = "Clear the PulseInfo cache", Notes = "")]
  [AllowAnonymous]
  public class ClearPulseInfoRequestDTO: IReturn<OkDTO>
  {
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

