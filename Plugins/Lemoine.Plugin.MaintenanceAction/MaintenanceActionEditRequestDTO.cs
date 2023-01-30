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
  [Api ("Request DTO for /MaintenanceAction/Edit service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/MaintenanceAction/Edit", "GET", Summary = "Service to edit a maintenance action", Notes = "To use with ?Id=&Version=&Title=&Description=")]
  [Route ("/MaintenanceAction/Edit/Get/{Id}/{Version}", "GET", Summary = "Service to edit a maintenance action", Notes = "")]
  public class MaintenanceActionEditRequestDTO
    : IReturn<MaintenanceActionEditResponseDTO>
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
    /// Maintenance action title
    /// </summary>
    [ApiMember (Name = "Title", Description = "Title", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Title { get; set; }

    /// <summary>
    /// Maintenance action description
    /// </summary>
    [ApiMember (Name = "Description", Description = "Description. Write 'remove' to remove the description", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Description { get; set; }

    /// <summary>
    /// In case of curative maintenance action, UTC stop date/time of the machine
    /// </summary>
    [ApiMember (Name = "StopDateTime", Description = "UTC stop date/time of the machine in case of curative maintenance action. Write 'remove' to remove an existing stop date/time", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string StopDateTime { get; set; }

    /// <summary>
    /// UTC planned date/time of the maintenance if known
    /// </summary>
    [ApiMember (Name = "PlannedDateTime", Description = "UTC planned date/time of the maintenance. Write 'remove' to remove an existing planned date/time", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string PlannedDateTime { get; set; }

    /// <summary>
    /// Remaining machining time in seconds before the maintenance action since the creation date/time
    /// </summary>
    [ApiMember (Name = "RemainingMachiningDurationSinceCreation", Description = "Remaining maching time in seconds before the maintenance action at the creation date/time. Write 0 to remove an existing value", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? RemainingMachiningDurationSinceCreation { get; set; }

    /// <summary>
    /// Standard machining time in seconds between two maintenance actions
    /// </summary>
    [ApiMember (Name = "StandardMachiningFrequency", Description = "Standard machining time in seconds between two maintenance actions. Write 0 to remove an existing value", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? StandardMachiningFrequency { get; set; }

    /// <summary>
    /// Standard total time in seconds between two maintenance actions
    /// </summary>
    [ApiMember (Name = "StandardTotalFrequency", Description = "Standard total time in seconds between two maintenance actions. Write 0 to remove an existing value", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? StandardTotalFrequency { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public MaintenanceActionEditRequestDTO ()
    {
    }
  }
}
