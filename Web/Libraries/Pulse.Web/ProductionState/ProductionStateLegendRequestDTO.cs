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

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ProductionStateLegend service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ProductionStateLegend/", "GET", Summary = "Get the legend for the production states", Notes = "")]
  [Route ("/ProductionState/Legend/", "GET", Summary = "Get the legend for the production states", Notes = "")]
  public class ProductionStateLegendRequestDTO : IReturn<ProductionStateLegendResponseDTO>
  {
  }
}
