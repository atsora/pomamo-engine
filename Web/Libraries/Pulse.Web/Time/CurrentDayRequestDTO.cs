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

namespace Pulse.Web.Time
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Time/CurrentDay service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Time/CurrentDay/", "GET", Summary = "Get the current day on the server", Notes = "")]
  public class CurrentDayRequestDTO: IReturn<CurrentDayResponseDTO>
  {
  }
}
