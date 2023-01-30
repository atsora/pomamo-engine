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
  [Api ("Request DTO for /MaintenanceAction/Finish service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/MaintenanceAction/Finish", "GET", Summary = "Service to finish a maintenance action", Notes = "To use with ?Id=&Version=")]
  [Route ("/MaintenanceAction/Finish/Get/{Id}/{Version}", "GET", Summary = "Service to finish a maintenance action", Notes = "")]
  public class MaintenanceActionFinishRequestDTO
    : IReturn<MaintenanceActionFinishResponseDTO>
  {
    /// <summary>
    /// Id of the maintenance action
    /// </summary>
    [ApiMember (Name = "Id", Description = "Id of the maintenance action", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Id { get; set; }

    /// <summary>
    /// Version of the maintenance action
    /// </summary>
    [ApiMember (Name = "Version", Description = "Version of the maintenance action", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Version { get; set; }

    /// <summary>
    /// Id of the machine (optional)
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id (optional): used to make the process faster only", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? MachineId { get; set; }

    /// <summary>
    /// Id of the machine (optional)
    /// </summary>
    [ApiMember (Name = "Renew", Description = "Renew the maintenance action in case of preventive maintenance action if the Standard times are set. Default: true", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool Renew { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public MaintenanceActionFinishRequestDTO ()
    {
      this.Renew = true;
    }
  }
}
