// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.GDBMigration;
using Lemoine.Core.Log;
using System.Data;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 339:
  /// </summary>
  [Migration(339)]
  public class AddToolPositionConstraint: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddToolPositionConstraint).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Clear the content of the tables toolposition and toollife
      Database.ExecuteNonQuery("DELETE FROM " + TableName.TOOL_LIFE);
      Database.ExecuteNonQuery("DELETE FROM " + TableName.TOOL_POSITION);
      
      // Add the column "toolid", which is a unique number representing the tool
      Database.AddColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "toolid",
                         DbType.String, ColumnProperty.NotNull);
      
      // Add a unique constraint on the table toolposition
      AddUniqueConstraint(TableName.TOOL_POSITION,
                          ColumnName.MACHINE_MODULE_ID,
                          TableName.TOOL_POSITION + "toolid");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove a unique constraint on the table toolposition
      try {
        RemoveUniqueConstraint(TableName.TOOL_POSITION,
                               ColumnName.MACHINE_MODULE_ID,
                               TableName.TOOL_POSITION + "toolid");
      } catch (Exception e) {
        log.ErrorFormat("Couldn't remove unique constraint on {0}: {1}",
                        TableName.TOOL_POSITION, e);
      }
      
      // Remove the column "toolid"
      Database.RemoveColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "toolid");
    }
  }
}
