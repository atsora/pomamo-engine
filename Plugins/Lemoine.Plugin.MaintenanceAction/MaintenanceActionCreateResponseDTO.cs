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
  [Api ("/MaintenanceAction/Create Response DTO")]
  public class MaintenanceActionCreateResponseDTO: MaintenanceActionLightDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maintenanceAction">not null</param>
    public MaintenanceActionCreateResponseDTO (MaintenanceAction maintenanceAction)
      : base (maintenanceAction)
    {
    }
  }
}
