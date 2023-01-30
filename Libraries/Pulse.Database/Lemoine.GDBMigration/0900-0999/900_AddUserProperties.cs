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
  /// Migration 900: add two columns "role" and "filter" to the table "usertable"
  /// </summary>
  [Migration (900)]
  public class AddUserTableProperties : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddUserTableProperties).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.USER, new Column (ColumnName.ROLE_ID, DbType.Int32));
      Database.AddColumn (TableName.USER, new Column (ColumnName.COMPANY_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.USER, ColumnName.ROLE_ID,
        TableName.ROLE, ColumnName.ROLE_ID,
        Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.USER, ColumnName.COMPANY_ID,
        TableName.COMPANY, ColumnName.COMPANY_ID,
        Migrator.Framework.ForeignKeyConstraint.SetNull);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.USER, ColumnName.ROLE_ID);
      Database.RemoveColumn (TableName.USER, ColumnName.COMPANY_ID);
    }
  }
}
