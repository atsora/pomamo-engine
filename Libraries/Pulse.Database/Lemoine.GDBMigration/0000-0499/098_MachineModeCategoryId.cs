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
  /// Migration 098: Add a new column MachineModeCategoryId to the MachineMode table
  /// </summary>
  [Migration(98)]
  public class MachineModeCategoryId: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeCategoryId).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODE,
                          new Column (ColumnName.MACHINE_MODE_CATEGORY_ID, DbType.Int32, ColumnProperty.NotNull, 1));
      Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecategoryid=2
WHERE machinemoderunning"); // Running
      Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecategoryid=3
WHERE machinemodeid=10"); // Error
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODE,
                             ColumnName.MACHINE_MODE_CATEGORY_ID);
    }
  }
}
