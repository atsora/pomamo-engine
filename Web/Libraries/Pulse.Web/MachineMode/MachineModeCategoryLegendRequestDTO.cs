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

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /MachineMode/CategoryLegend service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/MachineModeCategoryLegend/", "GET", Summary = "Get the legend for the machine mode categories", Notes = "")]
  [Route ("/MachineMode/CategoryLegend/", "GET", Summary = "Get the legend for the machine mode categories", Notes = "")]
  public class MachineModeCategoryLegendRequestDTO : IReturn<MachineModeCategoryLegendResponseDTO>
  {
  }
}

