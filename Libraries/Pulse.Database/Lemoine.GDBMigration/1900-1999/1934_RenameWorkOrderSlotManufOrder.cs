// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1934: complete the rename of task into manuforder in an upgraded database,
  /// so that its schema is the one of a new database
  ///
  /// - workorderslot.taskid becomes workorderslot.manuforderid, the column migration 278 creates in a new database
  /// - the function taskfulldisplay, left by migration 1901 when it renamed taskfull into manuforder1, is removed
  ///   (its replacement is manuforder1display)
  /// </summary>
  [Migration (1934)]
  public class RenameWorkOrderSlotManufOrder : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RenameWorkOrderSlotManufOrder).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.ColumnExists (TableName.WORK_ORDER_SLOT, ColumnName.TASK_ID)
        && !Database.ColumnExists (TableName.WORK_ORDER_SLOT, ColumnName.MANUFACTURING_ORDER_ID)) {
        RenamePartitionedColumn (TableName.WORK_ORDER_SLOT, ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID);
      }

      if (Database.TableExists (TableName.MANUFACTURING_ORDER_IMPLEMENTATION)) {
        Database.ExecuteNonQuery ($"""
          DROP FUNCTION IF EXISTS public.{TableName.TASK_FULL}display({TableName.MANUFACTURING_ORDER_IMPLEMENTATION});
          """);
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Nothing to do: these objects have the names migration 278 gives them in a new database
    }
  }
}
