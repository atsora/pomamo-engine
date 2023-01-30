// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 809: 
  /// </summary>
  [Migration (809)]
  public class ModificationStatusPriority2 : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ModificationStatusPriority2).FullName);

    static readonly string PRIORITY_FROM_MODIFICATION_TABLE_KEY = "Migration.809.PriorityFromModificationTable";
    static readonly bool PRIORITY_FROM_MODIFICATION_TABLE_DEFAULT = true;

    static readonly string COLUMN_NAME = "modificationstatuspriority";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Up (TableName.GLOBAL_MODIFICATION);
      Up (TableName.MACHINE_MODIFICATION);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

    void Up (string modificationTable)
    {
      string modificationStatusTable = modificationTable + "status";

      var priorityFromModificationTable = Lemoine.Info.ConfigSet
        .LoadAndGet (PRIORITY_FROM_MODIFICATION_TABLE_KEY, PRIORITY_FROM_MODIFICATION_TABLE_DEFAULT);
      if (priorityFromModificationTable) {
        if (modificationTable.StartsWith ("machine", StringComparison.InvariantCultureIgnoreCase)) {
          Database.ExecuteNonQuery ($@"
UPDATE {modificationStatusTable}
SET {COLUMN_NAME}=(SELECT modificationpriority FROM {modificationTable} WHERE {modificationStatusTable}.modificationid={modificationTable}.modificationid
  AND {modificationStatusTable}.{modificationStatusTable}machineid={modificationTable}.{modificationTable}machineid
)
");
        }
        else {
          Database.ExecuteNonQuery ($@"
UPDATE {modificationStatusTable}
SET {COLUMN_NAME}=(SELECT modificationpriority FROM {modificationTable} WHERE {modificationStatusTable}.modificationid={modificationTable}.modificationid)
");
        }
      }
      else {
        Database.ExecuteNonQuery ($@"
UPDATE {modificationStatusTable}
SET {COLUMN_NAME}=100
");
      }
    }
  }
}
