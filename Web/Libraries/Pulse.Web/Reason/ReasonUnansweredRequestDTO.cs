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
  [Api("Request DTO for /ReasonUnanswered service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonUnanswered/", "GET", Summary = "Check if there is an unanswered reason in the specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/ReasonUnanswered/Get/{MachineId}/{Range}", "GET", Summary = "Check if there is an unanswered reason in the specified range", Notes = "")]
  public class ReasonUnansweredRequestDTO: IReturn<ReasonUnansweredResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Date/time range (mandatory)
    /// </summary>
    [ApiMember(Name="Range", Description="Requested range", ParameterType="path", DataType="string", IsRequired=true)]
    public string Range { get; set; }
  }
}
