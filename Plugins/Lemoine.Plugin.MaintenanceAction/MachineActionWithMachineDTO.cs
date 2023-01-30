// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Pulse.Extensions.Web.Responses;
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
  public class MaintenanceActionWithMachineDTO: MaintenanceActionDTO
  {
    public MachineDTO Machine { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maintenanceAction">not null</param>
    public MaintenanceActionWithMachineDTO (MaintenanceAction maintenanceAction)
      : base (maintenanceAction)
    {
      this.Machine = new MachineDTO (maintenanceAction.Machine);
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="maintenanceAction">not null</param>
    /// <param name="estimatedDateTime">estimated date/time</param>
    public MaintenanceActionWithMachineDTO (MaintenanceAction maintenanceAction, DateTime? estimatedDateTime)
      : base (maintenanceAction, estimatedDateTime)
    {
      this.Machine = new MachineDTO (maintenanceAction.Machine);
    }
  }
}
