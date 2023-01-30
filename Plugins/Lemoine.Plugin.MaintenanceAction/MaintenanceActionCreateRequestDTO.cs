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
  [Api ("Request DTO for /MaintenanceAction/Create service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/MaintenanceAction/Create", "GET", Summary = "Service to create a maintenance action", Notes = "To use with ?MachineId=&Title=&Description=")]
  [Route ("/MaintenanceAction/Create/Get/{MachineId}/{Title}", "GET", Summary = "Service to create a maintenance action", Notes = "")]
  public class MaintenanceActionCreateRequestDTO
    : IReturn<MaintenanceActionCreateResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Maintenance action title
    /// </summary>
    [ApiMember (Name = "Title", Description = "Title", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Title { get; set; }

    /// <summary>
    /// Maintenance action title
    /// </summary>
    [ApiMember (Name = "Description", Description = "Description", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Description { get; set; }

    /// <summary>
    /// Maintenance action type
    /// </summary>
    [ApiMember (Name = "Maintenance action type", Description = "Curative or Preventive (default)", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Type { get; set; }

    /// <summary>
    /// In case of curative maintenance action, UTC stop date/time of the machine
    /// </summary>
    [ApiMember (Name = "StopDateTime", Description = "UTC stop date/time of the machine in case of curative maintenance action", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string StopDateTime { get; set; }

    /// <summary>
    /// UTC planned date/time of the maintenance if known
    /// </summary>
    [ApiMember (Name = "PlannedDateTime", Description = "UTC planned date/time of the maintenance", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string PlannedDateTime { get; set; }

    /// <summary>
    /// Remaining machining time in seconds before the maintenance action
    /// </summary>
    [ApiMember (Name = "RemainingMachiningDuration", Description = "Remaining maching time in seconds before the maintenance action at the creation date/time", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? RemainingMachiningDuration { get; set; }

    /// <summary>
    /// Standard machining time in seconds between two maintenance actions
    /// </summary>
    [ApiMember (Name = "StandardMachiningFrequency", Description = "Standard machining time in seconds between two maintenance actions", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? StandardMachiningFrequency { get; set; }

    /// <summary>
    /// Standard total time in seconds between two maintenance actions
    /// </summary>
    [ApiMember (Name = "StandardTotalFrequency", Description = "Standard total time in seconds between two maintenance actions", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? StandardTotalFrequency { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public MaintenanceActionCreateRequestDTO ()
    {
      this.Type = MaintenanceActionType.Preventive.ToString ();
    }
  }
}
