// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Scrap/At service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Scrap/At/", "GET", Summary = "Get the scrap status at the specified UTC date/time (default is now)", Notes = "To use with ?MachineId=&At=")]
  [Route ("/Scrap/At/Get", "GET", Summary = "Get the scrap status at the specified UTC date/time (default is now)", Notes = "To use with ?MachineId=&At=")]
  [Route ("/Scrap/At/Get/{MachineId}/{At}", "GET", Summary = "Get the scrap status at the specified UTC date/time (default is now)", Notes = "")]
  [Route ("/ScrapAt/", "GET", Summary = "Get the scrap status at the specified UTC date/time (default is now)", Notes = "To use with ?MachineId=&At=")]
  public class ScrapAtRequestDTO : IReturn<ScrapAtResponseDTO>
  {
    /// <summary>
    /// Id of the monitored machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// UTC date/time of the request
    /// </summary>
    [ApiMember (Name = "At", Description = "UTC date/time of the request in ISO format", ParameterType = "path", DataType = "string")]
    public string At { get; set; }
  }
}
