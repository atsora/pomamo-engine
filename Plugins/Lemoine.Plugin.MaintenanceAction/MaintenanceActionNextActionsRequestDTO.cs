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
  [Api ("Request DTO for /MaintenanceAction/NextActions service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/MaintenanceAction/NextActions", "GET", Summary = "Service to get the next maintenance actions to come", Notes = "To use with the optional ?Number=")]
  [Route ("/MaintenanceAction/NextActions/Get/{Number}", "GET", Summary = "Service to get the next maintenance actions to come", Notes = "")]
  public class MaintenanceActionNextActionsRequestDTO
    : IReturn<MaintenanceActionNextActionsResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "Number", Description = "Maximum number of actions to return. Default: 10", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? Number { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public MaintenanceActionNextActionsRequestDTO ()
    {
    }
  }
}
