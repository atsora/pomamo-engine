// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 353: add an index in the table EventToolLife, allowing us to find events
  /// related to a specific kind of event and for a specific tool in a machine module
  /// </summary>
  [Migration(353)]
  public class AddEventToolLifeIndex: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEventToolLifeIndex).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex(TableName.EVENT_TOOL_LIFE,
               ColumnName.MACHINE_MODULE_ID,
               TableName.EVENT_TOOL_LIFE + "typeid",
               TableName.EVENT_TOOL_LIFE + "toolid");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex(TableName.EVENT_TOOL_LIFE,
               ColumnName.MACHINE_MODULE_ID,
               TableName.EVENT_TOOL_LIFE + "typeid",
               TableName.EVENT_TOOL_LIFE + "toolid");
    }
  }
}
