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

namespace Pulse.Web.Info
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /PulseVersions service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/PulseVersions/", "GET", Summary = "Get all the versions", Notes = "")]
  [Route("/PulseVersions/Get", "GET", Summary = "Get all the versions", Notes = "")]
  [Route("/Info/PulseVersions/", "GET", Summary = "Get all the versions", Notes = "")]
  [Route("/Info/PulseVersions/Get", "GET", Summary = "Get all the versions", Notes = "")]
  [AllowAnonymous]
  public class PulseVersionsRequestDTO: IReturn<PulseVersionsResponseDTO>
  {
  }
}

