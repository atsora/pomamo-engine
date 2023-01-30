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
  [Api ("Request DTO for /MaintenanceAction/OpenMachineActions service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/MaintenanceAction/OpenMachineActions", "GET", Summary = "Service to list the open maintenance actions for a specified machine", Notes = "To use with ?MachineId=")]
  [Route ("/MaintenanceAction/OpenMachineActions/Get/{MachineId}", "GET", Summary = "Service to list the open maintenance actions for a specified machine", Notes = "")]
#if NSERVICEKIT
  [NServiceKit.ServiceHost.Route ("/MaintenanceAction/OpenMachineActions", "GET", Summary = "Service to list the open maintenance actions for a specified machine", Notes = "To use with ?MachineId=")]
  [NServiceKit.ServiceHost.Route ("/MaintenanceAction/OpenMachineActions/Get/{MachineId}", "GET", Summary = "Service to list the open maintenance actions for a specified machine", Notes = "")]
#endif // NSERVICEKIT
  public class MaintenanceActionOpenMachineActionsRequestDTO
    : IReturn<MaintenanceActionOpenMachineActionsResponseDTO>
#if NSERVICEKIT
    , NServiceKit.ServiceHost.IReturn<MaintenanceActionOpenMachineActionsResponseDTO>
#endif // NSERVICEKIT
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public MaintenanceActionOpenMachineActionsRequestDTO ()
    {
    }
  }
}
