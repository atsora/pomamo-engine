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
  /// Migration 1203: add a relationship between a deliverablepiece and a workorder 
  /// </summary>
  [Migration (1203)]
  public class DeliverablePieceWorkOrder : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DeliverablePieceWorkOrder).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.DELIVERABLE_PIECE,
        new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.DELIVERABLE_PIECE, ColumnName.WORK_ORDER_ID, TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID, Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.DELIVERABLE_PIECE, ColumnName.WORK_ORDER_ID);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.DELIVERABLE_PIECE, ColumnName.WORK_ORDER_ID);
    }
  }
}
