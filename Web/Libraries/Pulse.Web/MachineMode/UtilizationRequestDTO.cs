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
  /// Request DTO for the Utilization service
  /// 
  /// Return the machine % utilization for the specified day range or date/time range
  /// 
  /// To get the utilization target, use the UtilizationTarget service
  /// </summary>
  [Api("Request DTO for /Utilization service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Utilization/", "GET", Summary = "Get the utilization percentage in a specified range", Notes = "To use with ?MachineId=&DayRange= or ?MachineId=&Range=")]
  [Route("/Utilization/Get", "GET", Summary = "Get the utilization percentage in a specified range", Notes = "To use with ?MachineId=&DayRange= or ?MachineId=&Range=")]
  [Route("/Utilization/Get/{MachineId}/{DayRange}", "GET", Summary = "Get the running in a specified range", Notes = "")]
  [Route("/MachineMode/Utilization/", "GET", Summary = "Get the utilization percentage in a specified range", Notes = "To use with ?MachineId=&DayRange=")]
  [Route("/MachineMode/Utilization/Get", "GET", Summary = "Get the utilization percentage in a specified range", Notes = "To use with ?MachineId=&DayRange=")]
  [Route("/MachineMode/Utilization/Get/{MachineId}/{DayRange}", "GET", Summary = "Get the running in a specified range", Notes = "")]
  public class UtilizationRequestDTO: IReturn<UtilizationResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Day range if no Range is set
    /// 
    /// Default: "" that corresponds to [today, today] in case no Range is set
    /// </summary>
    [ApiMember(Name="DayRange", Description="Requested day range. Default is today.", ParameterType="path", DataType="string", IsRequired=false)]
    public string DayRange { get; set; }

    /// <summary>
    /// Range
    /// 
    /// It takes the priority on DayRange
    /// 
    /// Default: use DayRange instead
    /// </summary>
    [ApiMember(Name="Range", Description="Requested range. Default is use DayRange instead.", ParameterType="path", DataType="string", IsRequired=false)]
    public string Range { get; set; }
  }
}
