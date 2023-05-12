// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /MaintenanceAction/History service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/MaintenanceAction/History", "GET", Summary = "Service to list the open maintenance actions for a specified machine", Notes = "To use with ?MachineId=")]
  [Route ("/MaintenanceAction/History/Get/{MachineId}", "GET", Summary = "Service to list the open maintenance actions for a specified machine", Notes = "")]
  public class MaintenanceActionHistoryRequestDTO
    : IReturn<MaintenanceActionHistoryResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public MaintenanceActionHistoryRequestDTO ()
    {
    }
  }
}
