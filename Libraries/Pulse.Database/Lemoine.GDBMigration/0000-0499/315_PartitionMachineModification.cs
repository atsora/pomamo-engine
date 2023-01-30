// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 315: Partition the table machinemodification
  /// </summary>
  [Migration(315)]
  public class PartitionMachineModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PartitionMachineModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddForeignKeys ();
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS machinemodificationstatus_analysiscompletionorder_idx;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveForeignKeys ();
    }
    
    void AddForeignKeys ()
    {
      Database.GenerateForeignKey (TableName.LINK_OPERATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveForeignKeys ()
    {
      RemoveForeignKey (TableName.LINK_OPERATION, ColumnName.MACHINE_ID);
      RemoveForeignKey (TableName.REASON_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID);
    }
    
    void RemoveForeignKey (string tableName, string columnName)
    {
      Database.RemoveForeignKey (tableName, string.Format ("fk_{0}_{1}",
                                                           tableName, columnName));
    }
  }
}
