// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Complete maintenance action DTO
  /// </summary>
  public class MaintenanceActionDTO: MaintenanceActionLightDTO
  {
    /// <summary>
    /// Maintenance action title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Maintenance action description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Maintenance action type: Curative or Preventive
    /// </summary>
    public string MaintenanceActionType { get; set; }

    /// <summary>
    /// Maintenance action status: Open or Completed
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Creation date/time
    /// </summary>
    public string CreationDateTime { get; set; }

    /// <summary>
    /// Modification date/time
    /// </summary>
    public string ModifiedDateTime { get; set; }
    
    /// <summary>
    /// Completion date/time (may be empty)
    /// </summary>
    public string CompletionDateTime { get; set; }

    /// <summary>
    /// Optional stop date/time (for curative maintenance action)
    /// </summary>
    public string StopDateTime { get; set; }

    /// <summary>
    /// Optional planned date/time
    /// </summary>
    public string PlannedDateTime { get; set; }

    /// <summary>
    /// Remaining machining duration before the maintenance action at the creation date/time in seconds
    /// </summary>
    public int? RemainingMachiningDuration { get; set; }

    /// <summary>
    /// For preventive maintenance action, frequency at which the maintenance action should take place in machining duration in seconds
    /// </summary>
    public virtual int? StandardMachiningFrequency { get; set; }

    /// <summary>
    /// For preventive maintenance action, frequency at which the maintenance action should take place in seconds
    /// </summary>
    public virtual int? StandardTotalFrequency { get; set; }

    /// <summary>
    /// Estimated date/time if it could be determined
    /// </summary>
    public string EstimatedDateTime { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maintenanceAction">not null</param>
    public MaintenanceActionDTO (MaintenanceAction maintenanceAction)
      : base (maintenanceAction)
    {
      this.Title = maintenanceAction.Title;
      this.Description = maintenanceAction.Description;
      this.MaintenanceActionType = maintenanceAction.MaintenanceActionType.ToString ();
      this.Status = maintenanceAction.Status.ToString ();
      this.CreationDateTime = ConvertDTO.DateTimeUtcToIsoString (maintenanceAction.CreationDateTime);
      this.ModifiedDateTime = ConvertDTO.DateTimeUtcToIsoString (maintenanceAction.ModifiedDateTime);
      if (maintenanceAction.CompletionDateTime.HasValue) {
        this.CompletionDateTime = ConvertDTO.DateTimeUtcToIsoString (maintenanceAction.CompletionDateTime.Value);
      }
      if (maintenanceAction.StopDateTime.HasValue) {
        this.StopDateTime = ConvertDTO.DateTimeUtcToIsoString (maintenanceAction.StopDateTime.Value);
      }
      if (maintenanceAction.PlannedDateTime.HasValue) {
        this.PlannedDateTime = ConvertDTO.DateTimeUtcToIsoString (maintenanceAction.PlannedDateTime.Value);
      }
      if (maintenanceAction.RemainingMachiningDuration.HasValue) {
        this.RemainingMachiningDuration = (int)maintenanceAction.RemainingMachiningDuration.Value.TotalSeconds;
      }
      if (maintenanceAction.StandardMachiningFrequency.HasValue) {
        this.StandardMachiningFrequency = (int)maintenanceAction.StandardMachiningFrequency.Value.TotalSeconds;
      }
      if (maintenanceAction.StandardTotalFrequency.HasValue) {
        this.StandardTotalFrequency = (int)maintenanceAction.StandardTotalFrequency.Value.TotalSeconds;
      }
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="maintenanceAction">not null</param>
    /// <param name="estimatedDateTime">estimated date/time</param>
    public MaintenanceActionDTO (MaintenanceAction maintenanceAction, DateTime? estimatedDateTime)
      : this (maintenanceAction)
    {
      if (estimatedDateTime.HasValue) {
        this.EstimatedDateTime = ConvertDTO.DateTimeUtcToIsoString (estimatedDateTime.Value);
      }
    }
  }
}
