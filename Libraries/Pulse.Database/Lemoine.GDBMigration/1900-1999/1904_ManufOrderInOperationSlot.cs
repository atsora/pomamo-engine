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
  /// Migration 1904: rename task into manufacturing order in table operationslot
  /// </summary>
  [Migration (1904)]
  public class ManufOrderInOperationSlot : MigrationExt
  {
    static readonly string USE_DEPRECATED_TASK_KEY = "Migration.UseDeprecatedTask";
    static readonly bool USE_DEPRECATED_TASK_DEFAULT = false;

    static readonly ILog log = LogManager.GetLogger (typeof (ManufOrderInOperationSlot).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.ColumnExists (TableName.OPERATION_SLOT, $"{TableName.TASK}id")) {
        Database.RenameColumn (TableName.OPERATION_SLOT, $"{TableName.TASK}id", $"{TableName.MANUFACTURING_ORDER}id");
      }
      if (Database.ColumnExists (TableName.OPERATION_SLOT, $"{TableName.OPERATION_SLOT}autotask")) {
        Database.RenameColumn (TableName.OPERATION_SLOT, $"{TableName.OPERATION_SLOT}autotask", $"{TableName.OPERATION_SLOT}automanuforder");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)) {
        Database.RenameColumn (TableName.OPERATION_SLOT, $"{TableName.OPERATION_SLOT}automanuforder", $"{TableName.OPERATION_SLOT}autotask");
        Database.RenameColumn (TableName.OPERATION_SLOT, $"{TableName.MANUFACTURING_ORDER}id", $"{TableName.TASK}id");
      }
    }

  }
}
