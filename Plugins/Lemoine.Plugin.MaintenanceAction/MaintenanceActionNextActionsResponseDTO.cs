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
  [Api ("/MaintenanceActionNextActions Response DTO")]
  public class MaintenanceActionNextActionsResponseDTO
  {
    /// <summary>
    /// List of MaintenanceActionDTO, order by ascending estimated time
    /// </summary>
    public List<MaintenanceActionWithMachineDTO> Actions { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maintenanceActions">not null</param>
    public MaintenanceActionNextActionsResponseDTO (IEnumerable<MaintenanceActionWithMachineDTO> maintenanceActions)
    {
      this.Actions = maintenanceActions.ToList ();
    }
  }
}
