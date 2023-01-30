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
  /// Migration 233: add workorderline and workorderlinequantity tables
  /// </summary>
  [Migration(233)]
  public class AddWorkOrderLine: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddWorkOrderLine).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Create table WorkOrderLine
      CreateTableWorkOrderLine();
      
      // Create table WorkOrderLineQuantity
      CreateTableWordOrderLineQuantity();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable(TableName.WORK_ORDER_LINE_QUANTITY);
      Database.RemoveTable(TableName.WORK_ORDER_LINE);
    }
    
    void CreateTableWorkOrderLine()
    {
      Database.AddTable(TableName.WORK_ORDER_LINE,
                        new Column(ColumnName.WORK_ORDER_LINE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.WORK_ORDER_LINE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(ColumnName.LINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE + "begin", DbType.DateTime, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE + "end", DbType.DateTime),
                        new Column(TableName.WORK_ORDER_LINE + "deadline", DbType.DateTime, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE + "quantity", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.GenerateForeignKey(TableName.WORK_ORDER_LINE, ColumnName.WORK_ORDER_ID,
                                  TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.WORK_ORDER_LINE, ColumnName.LINE_ID,
                                  TableName.LINE_OLD, ColumnName.LINE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint(TableName.WORK_ORDER_LINE + "_unique", TableName.WORK_ORDER_LINE,
                                   ColumnName.WORK_ORDER_ID,
                                   ColumnName.LINE_ID);
    }
    
    void CreateTableWordOrderLineQuantity()
    {
      Database.AddTable(TableName.WORK_ORDER_LINE_QUANTITY,
                        new Column(ColumnName.WORK_ORDER_LINE_QUANTITY_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.WORK_ORDER_LINE_QUANTITY + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.WORK_ORDER_LINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(ColumnName.INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE_QUANTITY + "number", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.GenerateForeignKey(TableName.WORK_ORDER_LINE_QUANTITY, ColumnName.WORK_ORDER_LINE_ID,
                                  TableName.WORK_ORDER_LINE, ColumnName.WORK_ORDER_LINE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.WORK_ORDER_LINE_QUANTITY, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                  TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint(TableName.WORK_ORDER_LINE_QUANTITY + "_unique", TableName.WORK_ORDER_LINE_QUANTITY,
                            ColumnName.WORK_ORDER_LINE_ID,
                            ColumnName.INTERMEDIATE_WORK_PIECE_ID);
    }
  }
}
