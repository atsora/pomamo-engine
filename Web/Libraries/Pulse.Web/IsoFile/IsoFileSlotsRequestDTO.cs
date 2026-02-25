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

namespace Pulse.Web.IsoFile
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /IsoFileSlots service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/IsoFileSlots/", "GET", Summary = "Get the iso file slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/IsoFileSlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the iso file slots in a specified range", Notes = "")]
  [Route("/IsoFile/Slots/", "GET", Summary = "Get the iso file slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/IsoFile/Slots/Get/{MachineId}/{Range}", "GET", Summary = "Get the iso file slots in a specified range", Notes = "")]
  public class IsoFileSlotsRequestDTO: IReturn<IsoFileSlotsResponseDTO>
  {
    /// <summary>
    /// Id of the machine if you want to report only the iso file slots of the main machine module
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id if the iso file slots of the main machine module must be returned", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that corresponds to Today
    /// </summary>
    [ApiMember(Name="Range", Description="Requested range. Default is today. CurrentShift is an accepted parameter", ParameterType="path", DataType="string", IsRequired=false)]
    public string Range { get; set; }
    
    /// <summary>
    /// Skip the details in the answer
    /// 
    /// Default: false (return them)
    /// </summary>
    [ApiMember(Name="SkipDetails", Description="Skip the details in the answer. Default: false", ParameterType="path", DataType="boolean", IsRequired=false)]
    public bool SkipDetails { get; set; }
  }
}
