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

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /ReasonSelection service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonSelection/", "GET", Summary = "Get the reason selection in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/ReasonSelection/Get/{MachineId}/{Range}", "GET", Summary = "Get the reason selection in a specified range", Notes = "")]
  [Route("/Reason/ReasonSelection/", "GET", Summary = "Get the reason selection in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/Reason/ReasonSelection/Get/{MachineId}/{Range}", "GET", Summary = "Get the reason selection in a specified range", Notes = "")]
  public class ReasonSelectionRequestDTO: IReturn<ReasonSelectionResponseDTO>
  {
    /// <summary>
    /// Id of the monitored machine
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

  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ReasonSelection service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonSelection/Post", "POST", Summary="Get the reason selection from a list of specified ranges using a POST command", Notes = "To use with ?MachineId=")]
  [Route("/ReasonSelection/Post/{MachineId}", "POST", Summary="Get the reason selection from a list of specified ranges using a POST command")]
  [Route("/Reason/ReasonSelection/Post", "POST", Summary="Get the reason selection from a list of specified ranges using a POST command", Notes = "To use with ?MachineId=")]
  [Route("/Reason/ReasonSelection/Post/{MachineId}", "POST", Summary="Get the reason selection from a list of specified ranges using a POST command")]
  public class ReasonSelectionPostRequestDTO : IReturn<ReasonSelectionResponseDTO>
  {
    /// <summary>
    /// Id of the monitored machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }
}

