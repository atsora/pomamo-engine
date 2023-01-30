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
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.Tool
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /CurrentMachinesWithExpiredTools service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/CurrentMachinesWithExpiredTools/", "GET", Summary = "Get the list of machines with expired tools or with tools close to the expiration", Notes = "To be used with ?MaxRemainingDuration=")]
  [Route("/Tool/CurrentMachinesWithExpiredTools/", "GET",
         Summary = "Get the list of machines with expired tools or with tools close to the expiration",
         Notes = "To be used with ?MaxRemainingDuration=")]
  public class CurrentMachinesWithExpiredToolsRequestDTO: IReturn<List<MachineDTO>>
  {
    /// <summary>
    /// Maximum remaining duration filter before expiration in seconds
    /// 
    /// Default: 2h=7200s
    /// </summary>
    [ApiMember(Name="MaxRemainingDuration", Description="Maximum remaining duration before expiration in seconds", ParameterType="path", DataType="int", IsRequired=false)]
    public int? MaxRemainingDuration { get; set; }
  }
}
