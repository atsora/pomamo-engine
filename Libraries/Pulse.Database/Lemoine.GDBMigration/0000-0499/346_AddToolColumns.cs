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
  /// Migration 346: add the geometry and the departure time (if needed) in toolposition
  /// </summary>
  [Migration(346)]
  public class AddToolColumns: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddToolColumns).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Add 3 columns
      Database.AddColumn(TableName.TOOL_POSITION,
                         new Column(TableName.TOOL_POSITION + "leftdatetime", DbType.DateTime));
      Database.AddColumn(TableName.TOOL_POSITION,
                         new Column(TableName.TOOL_POSITION + "cuttercompensation", DbType.Double));
      Database.AddColumn(TableName.TOOL_POSITION,
                         new Column(TableName.TOOL_POSITION + "lengthcompensation", DbType.Double));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove 3 columns
      Database.RemoveColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "leftdatetime");
      Database.RemoveColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "cuttercompensation");
      Database.RemoveColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "lengthcompensation");
    }
  }
}
