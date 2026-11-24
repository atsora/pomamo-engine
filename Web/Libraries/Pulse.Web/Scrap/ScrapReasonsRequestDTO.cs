// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Scrap/Reasons service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Scrap/Reasons/", "GET", Summary = "Get the non conformance reasons that can be used to declare scrap on a machine", Notes = "To use with ?MachineId=")]
  [Route ("/Scrap/Reasons/Get/{MachineId}", "GET", Summary = "Get the non conformance reasons that can be used to declare scrap on a machine", Notes = "")]
  public class ScrapReasonsRequestDTO : IReturn<ScrapReasonsResponseDTO>
  {
    /// <summary>
    /// Id of the monitored machine
    ///
    /// Note: every non conformance reason applies to every machine for the moment
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }
}
