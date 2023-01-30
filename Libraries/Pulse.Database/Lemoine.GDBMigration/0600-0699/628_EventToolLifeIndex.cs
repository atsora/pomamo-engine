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
  /// Migration 628: 
  /// </summary>
  [Migration (628)]
  public class EventToolLifeIndex : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddAnalysisStatusNotApplicable).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_MODULE_ID, TableName.EVENT_TOOL_LIFE + "typeid",
        TableName.EVENT_TOOL_LIFE + "toolid");
      AddIndex (TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_MODULE_ID, TableName.EVENT_TOOL_LIFE + "typeid",
        TableName.EVENT_TOOL_LIFE + "toolid", "eventdatetime");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_MODULE_ID, TableName.EVENT_TOOL_LIFE + "typeid",
        TableName.EVENT_TOOL_LIFE + "toolid", "eventdatetime");
      AddIndex (TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_MODULE_ID, TableName.EVENT_TOOL_LIFE + "typeid",
        TableName.EVENT_TOOL_LIFE + "toolid");
    }
  }
}
