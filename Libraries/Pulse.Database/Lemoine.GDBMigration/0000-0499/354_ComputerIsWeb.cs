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
  /// Migration 354:
  /// </summary>
  [Migration(354)]
  public class ComputerIsWeb: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComputerIsWeb).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.COMPUTER,
                          new Column (TableName.COMPUTER + "isweb", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      Database.ExecuteNonQuery (@"
UPDATE computer
SET computerisweb=computerislctr;
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.COMPUTER, TableName.COMPUTER + "isweb");
    }
  }
}
