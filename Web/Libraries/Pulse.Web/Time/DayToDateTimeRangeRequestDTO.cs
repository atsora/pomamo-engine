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
  [Api("Request DTO for /Time/DayToDateTimeRange service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Time/DayToDateTimeRange/{Day}", "GET", Summary = "Get the date/time range for a specified day", Notes = "")]
  [Route ("/Time/DayToDateTimeRange/Get/{Day}", "GET", Summary = "Get the date/time range for a specified day", Notes = "")]
  [Route ("/Time/DayToDateTimeRange/", "GET", Summary = "Get the date/time range for a specified day", Notes = "To be used with ?Day=")]
  public class DayToDateTimeRangeRequestDTO: IReturn<DayToDateTimeRangeResponseDTO>
  {
    /// <summary>
    /// Day in ISO format
    /// </summary>
    public string Day { get; set; }
  }
}
