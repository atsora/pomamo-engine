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
  [Api("Request DTO for /RunningSlots service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/RunningSlots/", "GET", Summary = "Get the running slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/RunningSlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the running in a specified range", Notes = "")]
  [Route("/MachineMode/RunningSlots/", "GET", Summary = "Get the running slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/MachineMode/RunningSlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the running in a specified range", Notes = "")]
  public class RunningSlotsRequestDTO: IReturn<RunningSlotsResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that would correspond to [now, now]
    /// </summary>
    [ApiMember(Name="Range", Description="Requested range. Default is today. CurrentShift is an accepted parameter", ParameterType="path", DataType="string", IsRequired=false)]
    public string Range { get; set; }
  }
}
