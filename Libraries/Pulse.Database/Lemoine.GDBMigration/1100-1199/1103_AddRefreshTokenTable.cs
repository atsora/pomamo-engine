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
  /// Migration 1103: add the refreshtoken table
  /// </summary>
  [Migration (1103)]
  public class AddRefreshTokenTable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddRefreshTokenTable).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.REFRESH_TOKEN,
        new Column ($"{TableName.REFRESH_TOKEN}id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
        new Column ($"{TableName.REFRESH_TOKEN}version", DbType.Int32, ColumnProperty.NotNull, 1),
        new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.NotNull),
        new Column ($"{TableName.REFRESH_TOKEN}", DbType.String, ColumnProperty.NotNull),
        new Column ($"{TableName.REFRESH_TOKEN}creation", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
        new Column ($"{TableName.REFRESH_TOKEN}expiration", DbType.DateTime, ColumnProperty.NotNull),
        new Column ($"{TableName.REFRESH_TOKEN}revoked", DbType.DateTime),
        new Column ($"oauth2name", DbType.String),
        new Column ($"oauth2authenticationname", DbType.String),
        new Column ("oauth2refreshtoken", DbType.String)
      );
      this.MakeColumnText (TableName.REFRESH_TOKEN, TableName.REFRESH_TOKEN);
      this.MakeColumnText (TableName.REFRESH_TOKEN, "oauth2refreshtoken");
      Database.GenerateForeignKey (TableName.REFRESH_TOKEN, ColumnName.USER_ID, TableName.USER, ColumnName.USER_ID, Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndexCondition (TableName.REFRESH_TOKEN, $"{TableName.REFRESH_TOKEN}revoked IS NOT NULL", ColumnName.USER_ID);
      AddUniqueConstraint (TableName.REFRESH_TOKEN, TableName.REFRESH_TOKEN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.REFRESH_TOKEN);
    }
  }
}
