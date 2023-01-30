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
  /// Migration AddConfigActive: 
  /// </summary>
  [Migration (804)]
  public class AddConfigActive : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddConfigActive).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.CONFIG,
        new Column (TableName.CONFIG + "active", DbType.Boolean, ColumnProperty.Null, "TRUE"));
      Database.ExecuteNonQuery ($"UPDATE {TableName.CONFIG} SET {TableName.CONFIG}active=TRUE;");
      Database.ExecuteNonQuery ($@"UPDATE {TableName.CONFIG}
SET {TableName.CONFIG}active=FALSE
WHERE {TableName.CONFIG}value LIKE '%not set%'
;");
      SetNotNull (TableName.CONFIG, TableName.CONFIG + "active");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CONFIG, TableName.CONFIG + "active");
    }
  }
}
