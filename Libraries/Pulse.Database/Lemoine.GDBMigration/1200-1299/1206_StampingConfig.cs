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
  /// Migration 1206: add a new table stampingconfigbyname
  /// </summary>
  [Migration (1206)]
  public class AddStampingConfig : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddStampingConfig).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.STAMPING_CONFIG_BY_NAME,
                        new Column (ColumnName.STAMPING_CONFIG_BY_NAME_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column (TableName.STAMPING_CONFIG_BY_NAME + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column ("stampingconfigname", DbType.String, ColumnProperty.NotNull),
                        new Column (ColumnName.STAMPING_CONFIG, DbType.String, ColumnProperty.NotNull));
      MakeColumnCaseInsensitive (TableName.STAMPING_CONFIG_BY_NAME, "stampingconfigname");
      MakeColumnJson (TableName.STAMPING_CONFIG_BY_NAME, ColumnName.STAMPING_CONFIG); // Or MakeColumnText ?
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.STAMPING_CONFIG_BY_NAME);
    }
  }
}
