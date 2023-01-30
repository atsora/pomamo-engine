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

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /ReasonGroupLegend service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonGroupLegend/", "GET", Summary = "Get the legend for the reason groups", Notes = "")]
  [Route("/Reason/ReasonGroupLegend/", "GET", Summary = "Get the legend for the reason groups", Notes = "")]
  public class ReasonGroupLegendRequestDTO: IReturn<ReasonGroupLegendResponseDTO>
  {
  }
}

