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
  /// Migration 015: New tables to set some display options
  /// to get a display pattern for some other tables
  /// </summary>
  [Migration(15)]
  public class DisplayOptions: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DisplayOptions).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS display CASCADE;");
      if (!Database.TableExists (TableName.DISPLAY)) {
        Database.AddTable (TableName.DISPLAY,
                           new Column ("displaytable", DbType.String, ColumnProperty.PrimaryKey),
                           new Column ("displaypattern", DbType.String));
        Database.ExecuteNonQuery ("ALTER TABLE display " +
                                  "ALTER COLUMN displaytable " +
                                  "SET DATA TYPE CITEXT;");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.DISPLAY)) {
        Database.RemoveTable (TableName.DISPLAY);
      }
    }
  }
}
