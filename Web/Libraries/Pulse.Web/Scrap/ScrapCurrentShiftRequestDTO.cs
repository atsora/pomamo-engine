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
  [Api ("Request DTO for /Scrap/CurrentShift service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Scrap/CurrentShift/", "GET", Summary = "Get the scrap counters of the current shift (or day) of a machine", Notes = "To use with ?MachineId=")]
  [Route ("/Scrap/CurrentShift/Get/{MachineId}", "GET", Summary = "Get the scrap counters of the current shift (or day) of a machine", Notes = "")]
  public class ScrapCurrentShiftRequestDTO : IReturn<ScrapCurrentShiftResponseDTO>
  {
    /// <summary>
    /// Id of the monitored machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }
}
