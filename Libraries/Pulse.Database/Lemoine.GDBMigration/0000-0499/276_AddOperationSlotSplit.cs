// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 276:
  /// </summary>
  [Migration(276)]
  public class AddOperationSlotSplit: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationSlotSplit).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Cancel migration 274 first
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
DROP COLUMN IF EXISTS {1}",
                                               TableName.MACHINE_STATUS,
                                               "operationslotsplitend"));

      Database.AddTable (TableName.OPERATION_SLOT_SPLIT,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (TableName.OPERATION_SLOT_SPLIT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.OPERATION_SLOT_SPLIT + "end", DbType.DateTime));
      Database.GenerateForeignKey (TableName.OPERATION_SLOT_SPLIT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {0}{1}
SET DEFAULT now() AT TIME ZONE 'UTC';",
                                               TableName.OPERATION_SLOT_SPLIT,
                                               "end"));
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET {0}{1}=now() AT TIME ZONE 'UTC';",
                                               TableName.OPERATION_SLOT_SPLIT,
                                               "end"));
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_SLOT_SPLIT,
                                               "end"));
      
      PartitionTable (TableName.OPERATION_SLOT_SPLIT, TableName.MACHINE);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.OPERATION_SLOT_SPLIT);
    }
  }
}
