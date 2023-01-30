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
  [Api ("Request DTO for /CncAlarm/Current service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/CncAlarm/Current/", "GET", Summary = "Get a current cnc alarm", Notes = "To use with ?MachineId=")]
  [Route ("/CncAlarm/Current/Get/ByMachine/{MachineId}", "GET", Summary = "Get a current cnc alarm for the specified machine ID")]
  public class CncAlarmCurrentRequestDTO : IReturn<CncAlarmCurrentResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id to target the main machine module", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int MachineId { get; set; }

    /// <summary>
    /// Id of the machine module
    /// </summary>
    [ApiMember (Name = "MachineModuleId", Description = "Machine Module Id. You must set either a MachineId or a MachineModuleId. If not set, the main machine module of the specified MachineId is considered", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int MachineModuleId { get; set; }

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
    [ApiMember (Name ="IncludeIgnored", Description = "Include the CNC alarms with the property Focus=false (ignored). Default is false", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool IncludeIgnored { get; set; }
  }
}
