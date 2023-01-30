// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Security.Cryptography.X509Certificates;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Column name constants
  /// 
  /// Warning ! Try to prefix the column name by the main table name to ease the use of the natural joins
  /// </summary>
  internal class ColumnName: Lemoine.GDBMigration.ColumnName
  {
    /// <summary>
    /// Id of the maintenanceaction table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION_ID = TableName.MAINTENANCE_ACTION + "id";

    /// <summary>
    /// Id of the maintenanceactiontype table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION_TYPE_ID = TableName.MAINTENANCE_ACTION + "typeid";

    /// <summary>
    /// Id of the maintenanceactionstatus table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION_STATUS_ID = TableName.MAINTENANCE_ACTION + "statusid";

    /// <summary>
    /// Id of the maintenanceactionupdate table
    /// </summary>
    public static readonly string MAINTENANCE_ACTION_UPDATE_ID = TableName.MAINTENANCE_ACTION_UPDATE + "id";
  }
}
