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
  /// Migration 147: in migration 145, ChangeColumn changed the order of the columns,
  /// which had an impact on C++ code. Re-order the columns
  /// </summary>
  [Migration(147)]
  public class CorrectMachineModeColumnOrder: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CorrectMachineModeColumnOrder).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Database.ChangeColumn does not work well with PostgreSQL
      // Hopefully this fix for Migration 145 is not needed any more
      /*
      Database.ChangeColumn (TableName.MACHINE_MODE,
                             new Column (TableName.MACHINE_MODE + "version", DbType.Int32, ColumnProperty.NotNull, 1));
      Database.ChangeColumn (TableName.MACHINE_MODE,
                             new Column (ColumnName.MACHINE_MODE_CATEGORY_ID, DbType.Int32, ColumnProperty.NotNull, 1));
      */
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
