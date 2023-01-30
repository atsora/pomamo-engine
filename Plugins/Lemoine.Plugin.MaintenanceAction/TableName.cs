// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Table name constants
  /// </summary>
  internal class TableName: Lemoine.GDBMigration.TableName
  {
    /// <summary>
    /// maintenanceaction table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION = "maintenanceaction";
    /// <summary>
    /// maintenanceactiontype table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION_TYPE = "maintenanceactiontype";
    /// <summary>
    /// maintenanceactionstatus table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION_STATUS = "maintenanceactionstatus";
    /// <summary>
    /// maintenanceactionupdate table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION_UPDATE = "maintenanceactionupdate";
  }
}
