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
  /// Request DTO of the UtilizationTarget service
  /// 
  /// Service to get only the % utilization target.
  /// 
  /// To get the % utilization, use the Utilization service
  /// </summary>
  [Api("Request DTO for /UtilizationTarget service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/UtilizationTarget/", "GET", Summary = "Get the utilization target", Notes = "To use with ?MachineId=")]
  [Route("/UtilizationTarget/{MachineId}", "GET", Summary = "Get the utilization target", Notes = "")]
  [Route("/UtilizationTarget/Get", "GET", Summary = "Get the utilization target", Notes = "To use with ?MachineId=")]
  [Route("/UtilizationTarget/Get/{MachineId}", "GET", Summary = "Get the utilization target", Notes = "")]
  [Route("/MachineMode/UtilizationTarget/", "GET", Summary = "Get the utilization target", Notes = "To use with ?MachineId=")]
  [Route("/MachineMode/UtilizationTarget/{MachineId}", "GET", Summary = "Get the utilization target", Notes = "")]
  [Route("/MachineMode/UtilizationTarget/Get", "GET", Summary = "Get the utilization target", Notes = "To use with ?MachineId=")]
  [Route("/MachineMode/UtilizationTarget/Get/{MachineId}", "GET", Summary = "Get the utilization target", Notes = "")]
  public class UtilizationTargetRequestDTO: IReturn<UtilizationTargetResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
  }
}

