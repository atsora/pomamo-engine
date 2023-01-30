// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Maintenance action type
  /// </summary>
  public enum MaintenanceActionType
  {
    /// <summary>
    /// Preventive maintenance
    /// </summary>
    Preventive = 1,
    /// <summary>
    /// Curative maintenance
    /// </summary>
    Curative = 2,
  }
}
