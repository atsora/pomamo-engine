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
  /// Migration 1202: add the relationship Project <-> customer 
  /// </summary>
  [Migration (1202)]
  public class ProjectCustomer : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProjectCustomer).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.PROJECT,
        new Column (ColumnName.CUSTOMER_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.PROJECT, ColumnName.CUSTOMER_ID, TableName.CUSTOMER, ColumnName.CUSTOMER_ID, Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.PROJECT, ColumnName.CUSTOMER_ID);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.PROJECT, ColumnName.CUSTOMER_ID);
    }
  }
}
