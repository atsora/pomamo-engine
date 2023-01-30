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
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.MachineStateTemplate
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /MachineStateTemplateSlots service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/MachineStateTemplateSlots/", "GET", Summary = "Get the machine state template slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/MachineStateTemplateSlots/{MachineId}/{Range}", "GET", Summary = "Get the machine state template slots in a specified range", Notes = "")]
  [Route("/MachineStateTemplate/Slots/", "GET", Summary = "Get the machine state template slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/MachineStateTemplate/Slots/{MachineId}/{Range}", "GET", Summary = "Get the machine state template slots in a specified range", Notes = "")]
  public class MachineStateTemplateSlotsRequestDTO: IReturn<MachineStateTemplateSlotsResponseDTO>
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
    [ApiMember(Name="Range", Description="Requested range. Default is [now, now]", ParameterType="path", DataType="string", IsRequired=false)]
    public string Range { get; set; }
  }
}
