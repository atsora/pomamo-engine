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
  [Api ("/MaintenanceAction/Finish Response DTO")]
  public class MaintenanceActionFinishResponseDTO: MaintenanceActionLightDTO
  {
    /// <summary>
    /// Was the maintenance action renewed ? If it is true, then the renewed maintenance action id is returned
    /// </summary>
    public bool Renew { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maintenanceAction">not null</param>
    /// <param name="renew"></param>
    public MaintenanceActionFinishResponseDTO (MaintenanceAction maintenanceAction, bool renew)
      : base (maintenanceAction)
    {
      this.Renew = renew;
    }
  }
}
