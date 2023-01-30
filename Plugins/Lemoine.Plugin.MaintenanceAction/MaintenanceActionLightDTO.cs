// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Light maintenance action DTO (only Id and Version)
  /// </summary>
  public class MaintenanceActionLightDTO
  {
    /// <summary>
    /// MaintenanceAction Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// MaintenanceAction Version
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maintenanceAction">not null</param>
    public MaintenanceActionLightDTO (MaintenanceAction maintenanceAction)
    {
      Debug.Assert (null != maintenanceAction);

      this.Id = maintenanceAction.Id;
      this.Version = maintenanceAction.Version;
    }
  }
}
