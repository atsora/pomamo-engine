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

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /SequenceSlots service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/SequenceSlots/", "GET", Summary = "Get the sequence slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/SequenceSlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the sequence slots in a specified range", Notes = "")]
  [Route("/Operation/SequenceSlots/", "GET", Summary = "Get the sequence slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/Operation/SequenceSlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the sequence slots in a specified range", Notes = "")]
  public class SequenceSlotsRequestDTO: IReturn<SequenceSlotsResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
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
    [ApiMember(Name="SkipDetails", Description="Skip the details in the answer. Default: false", ParameterType="path", DataType="bool", IsRequired=false)]
    public bool SkipDetails { get; set; }
  }
}
