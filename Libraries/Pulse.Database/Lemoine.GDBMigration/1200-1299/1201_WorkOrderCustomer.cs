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
  /// Migration 1201: add the relationship workorder <-> customer 
  /// </summary>
  [Migration (1201)]
  public class WorkOrderCustomer : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WorkOrderCustomer).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.WORK_ORDER,
        new Column (ColumnName.CUSTOMER_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.WORK_ORDER, ColumnName.CUSTOMER_ID, TableName.CUSTOMER, ColumnName.CUSTOMER_ID, Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.WORK_ORDER, ColumnName.CUSTOMER_ID);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.WORK_ORDER, ColumnName.CUSTOMER_ID);
    }
  }
}
