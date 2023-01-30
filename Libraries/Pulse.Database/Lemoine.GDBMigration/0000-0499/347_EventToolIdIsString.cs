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
  /// Migration 347: fix the column ToolId which was an int (it is a string)
  /// </summary>
  [Migration(347)]
  public class EventToolIdIsString: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EventToolIdIsString).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Change the column type
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "toolid");
      Database.AddColumn(TableName.EVENT_TOOL_LIFE,
                         new Column(TableName.EVENT_TOOL_LIFE + "toolid", DbType.String));
      
      // Remove unique constraint
      RemoveUniqueConstraint(TableName.TOOL_POSITION, ColumnName.MACHINE_MODULE_ID,
                             TableName.TOOL_POSITION + "magazine",
                             TableName.TOOL_POSITION + "pot");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "toolid");
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "toolid",
                         DbType.Int32, 4, ColumnProperty.NotNull, -1);
    }
  }
}
