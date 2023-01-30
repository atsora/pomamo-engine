// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.CncAlarm
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /CncAlarm/At service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/CncAlarm/At/", "GET", Summary = "Get the cnc values at a specified UTC date/time", Notes = "To use with ?MachineId=&At=")]
  [Route("/CncAlarm/At/Get/{MachineId}/{At}", "GET", Summary = "Get the cnc values at a specitified date/time for the specified machine ID")]
  public class CncAlarmAtRequestDTO: IReturn<CncAlarmAtResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id in case the main machine module is targetted", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Date/time
    /// </summary>
    [ApiMember(Name="At", Description="Requested date/time", ParameterType="path", DataType="string", IsRequired=true)]
    public string At { get; set; }

    /// <summary>
    /// Keep only the cnc alarms with a focus status
    /// 
    /// Default is False, return the cnc alarms with an unknown status too
    /// </summary>
    [ApiMember (Name = "KeepFocusOnly", Description = "Keep only the CNC alarms with property Focus=true. Default is false, return the cnc alarms with an unknown status too", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool KeepFocusOnly { get; set; }

    /// <summary>
    /// Include the ignored cnc alarms (Focus is false)
    /// 
    /// Default is False
    /// </summary>
    [ApiMember (Name = "IncludeIgnored", Description = "Include the CNC alarms with the property Focus=false (ignored). Default is false", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool IncludeIgnored { get; set; }
  }
}
