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

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /CncValueLegend service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/CncValueLegend", "GET", Summary = "Get the cnc value / field legends for the specified machines", Notes = "To use with ?MachineIds=. The list of machine ids is comma separated")]
  [Route ("/CncValueLegend/Get", "GET", Summary = "Get the cnc value / field legends for the specified machines", Notes = "To use with ?MachineIds=. The list of machine ids is comma separated")]
  [Route ("/CncValueLegend/Get/{MachineIds}", "GET", Summary = "Get the cnc value / field legends for the specified machines", Notes = "The list of machine ids is comma separated")]
  [Route ("/CncValue/Legend", "GET", Summary = "Get the cnc value / field legends for the specified machines", Notes = "To use with ?MachineIds=. The list of machine ids is comma separated")]
  [Route ("/CncValue/Legend/Get", "GET", Summary = "Get the cnc value / field legends for the specified machines", Notes = "To use with ?MachineIds=. The list of machine ids is comma separated")]
  [Route ("/CncValue/Legend/Get/{MachineIds}", "GET", Summary = "Get the cnc value / field legends for the specified machines", Notes = "The list of machine ids is comma separated")]
  public class CncValueLegendRequestDTO : IReturn<CncValueLegendResponseDTO>
  {
    /// <summary>
    /// Ids of the machine
    /// </summary>
    [ApiMember (Name = "MachineIds", Description = "Machine Ids", ParameterType = "path", DataType = "List(int)", IsRequired = true)]
    public IList<int> MachineIds { get; set; }
  }
}

