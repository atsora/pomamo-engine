// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ReasonManualOrOverwriteSlots service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ReasonManualOrOverwriteSlots/", "GET", Summary = "Get the reason manual or overwrite slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route ("/ReasonManualOrOverwriteSlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the reason manual or overwrite slots in a specified range", Notes = "")]
  [Route ("/Reason/ManualOrOverwriteSlots/", "GET", Summary = "Get the reason manual or overwrite slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route ("/Reason/ManualOrOverwriteSlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the reason manual or overwrite slots in a specified range", Notes = "")]
  public class ReasonManualOrOverwriteSlotsRequestDTO : IReturn<ReasonManualOrOverwriteSlotsResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that would correspond to [now, now]
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range. Default is [now, now]", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }
  }
}
