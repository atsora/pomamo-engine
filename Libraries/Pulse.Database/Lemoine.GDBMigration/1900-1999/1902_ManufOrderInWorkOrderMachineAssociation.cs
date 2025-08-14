// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1902: rename task into manufacturing order in table workordermachineassociation
  /// </summary>
  [Migration (1902)]
  public class ManufOrderInWorkOrderMachineAssociation : MigrationExt
  {
    static readonly string USE_DEPRECATED_TASK_KEY = "Migration.UseDeprecatedTask";
    static readonly bool USE_DEPRECATED_TASK_DEFAULT = false;

    static readonly ILog log = LogManager.GetLogger (typeof (ManufOrderInWorkOrderMachineAssociation).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.ColumnExists (TableName.WORKORDER_MACHINE_ASSOCIATION, $"{TableName.WORKORDER_MACHINE_ASSOCIATION}resettask")) {
        Database.RenameColumn (TableName.WORKORDER_MACHINE_ASSOCIATION, $"{TableName.WORKORDER_MACHINE_ASSOCIATION}resettask", $"{TableName.WORKORDER_MACHINE_ASSOCIATION}resetmanuforder");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)) {
        Database.RenameColumn (TableName.WORKORDER_MACHINE_ASSOCIATION, $"{TableName.WORKORDER_MACHINE_ASSOCIATION}resetmanuforder", $"{TableName.WORKORDER_MACHINE_ASSOCIATION}resettask");
      }
    }

  }
}
