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

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /MachineModeColorLegend service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/MachineModeColorLegend/", "GET", Summary = "Get the legend for the machine modes", Notes = "")]
  [Route("/MachineModeColorLegend/Get", "GET", Summary = "Get the legend for the machine modes", Notes = "")]
  [Route("/MachineMode/ColorLegend/", "GET", Summary = "Get the legend for the machine modes", Notes = "")]
  [Route("/MachineMode/ColorLegend/Get", "GET", Summary = "Get the legend for the machine modes", Notes = "")]
  public class MachineModeColorLegendRequestDTO: IReturn<MachineModeColorLegendResponseDTO>
  {
  }
}
