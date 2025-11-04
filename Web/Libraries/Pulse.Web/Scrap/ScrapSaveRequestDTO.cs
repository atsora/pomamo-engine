// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.CommonResponseDTO;
using Pulse.Web.Reason;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for ScrapSave service (not used)")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Scrap/ScrapSave", "GET", Summary = "Unused service to save a scrap report", Notes = "To use with ?MachineId=")]
  public class ScrapSaveRequestDTO : IReturn<ScrapSaveResponseDTO>
  {
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }

  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for ReasonSave service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ScrapSave/Post", "POST", Summary = "Service to save a scrap report", Notes = "To use with ?MachineId")]
  [Route ("/ScrapSave/Post/{MachineId}", "POST", Summary = "Service to save a scrap report", Notes = "")]
  [Route ("/Scrap/ScrapSave/Post", "POST", Summary = "Service to save a scrap report", Notes = "To use with ?MachineId")]
  [Route ("/Scrap/ScrapSave/Post/{MachineId}", "POST", Summary = "Service to save a scrap report", Notes = "")]
  public class ScrapSavePostRequestDTO : IReturn<ScrapSaveResponseDTO>
  {
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }
}
