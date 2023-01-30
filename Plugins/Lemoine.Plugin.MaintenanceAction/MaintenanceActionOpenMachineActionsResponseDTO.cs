// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Lemoine.Extensions.Web.Attributes;

namespace Lemoine.Plugin.MaintenanceAction
{
  [Api ("/MaintenanceAction/OpenMachineActions Response DTO")]
  public class MaintenanceActionOpenMachineActionsResponseDTO
  {
    /// <summary>
    /// List of MaintenanceActionDTO, order by ascending estimated time
    /// </summary>
    public List<MaintenanceActionDTO> Actions { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maintenanceActions">not null</param>
    public MaintenanceActionOpenMachineActionsResponseDTO (IEnumerable<MaintenanceActionDTO> maintenanceActions)
    {
      this.Actions = maintenanceActions.ToList ();
    }
  }
}
