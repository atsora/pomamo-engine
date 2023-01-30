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

namespace Lemoine.Web.ProductionMachiningStatus.CurrentMachineStateTemplateOperation
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /CurrentMachineStateTemplateOperation service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/CurrentMachineStateTemplateOperation/", "GET", Summary = "Get the current machine state template / operation for the specific machine", Notes = "To use with ?MachineId=")]
  [Route("/CurrentMachineStateTemplateOperation/{MachineId}", "GET", Summary = "Get the current machine state template / operation for the specific machine", Notes = "")]
  public class CurrentMachineStateTemplateOperationRequestDTO: IReturn<System.Collections.Generic.IEnumerable<CurrentMachineStateTemplateOperationResponseDTO>>
  {
    /// <summary>
    /// Id of requested reason
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Applicable date/time
    /// 
    /// Default: now
    /// </summary>
    [ApiMember(Name="DateTime", Description="Applicable date/time", ParameterType="path", DataType="string", IsRequired=false)]
    public DateTime? DateTime { get; set; }
  }
}
