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
  /// Migration 807: add a cellkind column
  /// </summary>
  [Migration (807)]
  public class CellKind : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CellKind).FullName);

    static readonly string COLUMN_NAME = TableName.CELL + "kind";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.CELL,
        new Column (COLUMN_NAME, DbType.Int32));
      Database.ExecuteNonQuery ($@"UPDATE {TableName.CELL}
SET {COLUMN_NAME}=0");
      SetNotNull (TableName.CELL, COLUMN_NAME);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CELL, COLUMN_NAME);
    }
  }
}
