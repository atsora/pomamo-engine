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
  /// Migration 143: add frequency column in sequence table
  /// </summary>
  [Migration(143)]
  public class AddSequenceFrequencyColumn: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddSequenceFrequencyColumn).FullName);
    static readonly string columnName = TableName.SEQUENCE + "frequency";
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.SEQUENCE,
                          new Column (columnName, DbType.Int32, ColumnProperty.NotNull, 1));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.SEQUENCE, columnName);
    }
  }
}
