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
  /// Migration 344: add the column "toolid" in the table eventtoollife
  /// </summary>
  [Migration(344)]
  public class AddEventToolId: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEventToolId).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "toolid",
                         DbType.Int32, 4, ColumnProperty.NotNull, -1);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "toolid");
    }
  }
}
