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
  /// Migration 044: Add a config table
  /// </summary>
  [Migration(44)]
  public class AddConfigTable: MigrationExt
  {
    static readonly string CONFIG_ID = "configid";
    static readonly string CONFIG_VERSION = "configversion";
    
    static readonly ILog log = LogManager.GetLogger(typeof (AddConfigTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.CONFIG,
                         new Column (CONFIG_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (CONFIG_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ("configkey", DbType.String, ColumnProperty.NotNull | ColumnProperty.Unique),
                         new Column ("configvalue", DbType.Binary, ColumnProperty.NotNull));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.CONFIG);
    }
  }
}
