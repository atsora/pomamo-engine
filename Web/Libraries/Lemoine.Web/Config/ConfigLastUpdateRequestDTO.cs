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

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Config/LastUpdate service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Config/LastUpdate", "GET", Summary = "Get the last configuration update date/time", Notes = "")]
  [AllowAnonymous]
  public class ConfigLastUpdateRequestDTO: IReturn<ConfigLastUpdateResponseDTO>
  {
  }
}

