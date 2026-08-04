// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1802: add CycleDelta column to the toollife table
  /// </summary>
  [Migration(1802)]
  public class AddCycleDeltaToToolLife: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCycleDeltaToToolLife).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Add a nullable column "toollifecycledelta" to the table toollife
      Database.AddColumn(TableName.TOOL_LIFE,
                         new Column(TableName.TOOL_LIFE + "cycledelta",
                                    DbType.Double));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Remove the column "toollifecycledelta" from toollife
      Database.RemoveColumn(TableName.TOOL_LIFE, TableName.TOOL_LIFE + "cycledelta");
    }
  }
}
