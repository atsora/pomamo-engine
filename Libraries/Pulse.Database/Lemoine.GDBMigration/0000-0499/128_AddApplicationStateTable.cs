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
  /// Migration 128: Add a table to store the state of various applications
  /// 
  /// The table is very similar to the config table but it keeps the current state of the applications
  /// </summary>
  [Migration(128)]
  public class AddApplicationStateTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddApplicationStateTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.APPLICATION_STATE,
                         new Column (TableName.APPLICATION_STATE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.APPLICATION_STATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.APPLICATION_STATE + "key", DbType.String, ColumnProperty.NotNull | ColumnProperty.Unique),
                         new Column (TableName.APPLICATION_STATE + "value", DbType.String, ColumnProperty.NotNull));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.APPLICATION_STATE);
    }
  }
}
