// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;
using Lemoine.GDBMigration;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 320:
  /// </summary>
  [Migration(320)]
  public class AddToolConstraints: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddToolConstraints).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Delete tool lifes with no position
      Database.ExecuteNonQuery(String.Format(
        "DELETE FROM {0} WHERE {1} IS NULL;",
        TableName.TOOL_LIFE, ColumnName.TOOL_POSITION_ID));
      
      // Unpartition if possible
      UnpartitionTable(TableName.TOOL_LIFE);
      UnpartitionTable(TableName.TOOL_POSITION);
      
      // Not null constraint
      try {
        Database.ExecuteNonQuery(String.Format(
          "ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL;",
          TableName.TOOL_LIFE, ColumnName.TOOL_POSITION_ID));
      } catch (Exception e) {
        // May fail if already not null
        log.ErrorFormat("Maybe tool position id already not null: {0}", e);
      }
      
      // Remove and restore constraint
      try {
        Database.RemoveForeignKey(TableName.TOOL_LIFE, "fk_toollife_toolposition");
        Database.GenerateForeignKey(TableName.TOOL_LIFE, ColumnName.TOOL_POSITION_ID,
                                    TableName.TOOL_POSITION, ColumnName.TOOL_POSITION_ID,
                                    ForeignKeyConstraint.Cascade);
      } catch (Exception e) {
        log.ErrorFormat("Couldn't update foreign key tool position for ToolLife table: {0}", e);
      }
      
      // Partition the tables
      try {
        PartitionTable(TableName.TOOL_LIFE, TableName.MACHINE_MODULE);
        PartitionTable(TableName.TOOL_POSITION, TableName.MACHINE_MODULE);
      } catch (Exception e) {
        log.ErrorFormat("Couldn't partition ToolLife and ToolPosition tables, maybe already done: {0}", e);
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Nothing
    }
  }
}
