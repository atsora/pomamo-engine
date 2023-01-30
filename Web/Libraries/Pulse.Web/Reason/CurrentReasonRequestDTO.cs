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

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /CurrentReason service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/CurrentReason/", "GET", Summary = "Get the current machine mode and reason", Notes = "To use with ?MachineId=")]
  [Route("/CurrentReason/Get/{MachineId}", "GET", Summary = "Get the current machine mode and reason", Notes = "")]
  [Route("/Reason/CurrentReason/", "GET", Summary = "Get the current machine mode", Notes = "To use with ?MachineId=")]
  [Route("/Reason/CurrentReason/Get/{MachineId}", "GET", Summary = "Get the current machine mode", Notes = "")]
  public class CurrentReasonRequestDTO: IReturn<CurrentReasonResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Period to consider for the returned start date/time:
    /// <item>none</item>
    /// <item>reason_machinemodecategory</item>
    /// <item>running</item>
    /// <item>running_machinemodecategory</item>
    /// </summary>
    [ApiMember (Name="Period", Description="none|reason_machinemodecategory|running|running_machinemodecategory: period to consider for the start date/time", ParameterType="path", DataType="string", IsRequired=false)]
    public string Period { get; set; }

    /// <summary>
    /// Do not return the duration if the machine is running
    /// </summary>
    [ApiMember (Name="NotRunningOnlyDuration", Description="Do not return the duration if the machine is running", ParameterType="path", DataType="boolean", IsRequired=false)]
    public bool NotRunningOnlyDuration { get; set; }
  }
}

