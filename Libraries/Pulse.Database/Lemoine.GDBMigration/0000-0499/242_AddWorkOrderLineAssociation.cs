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
  /// Migration 242:
  /// </summary>
  [Migration(242)]
  public class AddWorkOrderLineAssociation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddWorkOrderLineAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CreateTableWorkOrderLineAssociation ();
      CreateTableWordOrderLineAssociationQuantity ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable(TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY);
      Database.RemoveTable(TableName.WORK_ORDER_LINE_ASSOCIATION);
    }
    
    void CreateTableWorkOrderLineAssociation()
    {
      Database.AddTable(TableName.WORK_ORDER_LINE_ASSOCIATION,
                        new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(ColumnName.WORK_ORDER_ID, DbType.Int32),
                        new Column(ColumnName.LINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE_ASSOCIATION + "begin", DbType.DateTime, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE_ASSOCIATION + "end", DbType.DateTime),
                        new Column(TableName.WORK_ORDER_LINE_ASSOCIATION + "deadline", DbType.DateTime, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE_ASSOCIATION + "quantity", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.GenerateForeignKey (TableName.WORK_ORDER_LINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.WORK_ORDER_LINE_ASSOCIATION, ColumnName.WORK_ORDER_ID,
                                  TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.WORK_ORDER_LINE_ASSOCIATION, ColumnName.LINE_ID,
                                  TableName.LINE_OLD, ColumnName.LINE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.WORK_ORDER_LINE_ASSOCIATION);
    }
    
    void CreateTableWordOrderLineAssociationQuantity()
    {
      Database.AddTable(TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY,
                        new Column(TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.Null),
                        new Column(ColumnName.INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY + "number", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.GenerateForeignKey (TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY, ColumnName.MODIFICATION_ID,
                                   TableName.WORK_ORDER_LINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                  TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint(TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY + "_unique", TableName.WORK_ORDER_LINE_ASSOCIATION_QUANTITY,
                            ColumnName.MODIFICATION_ID,
                            ColumnName.INTERMEDIATE_WORK_PIECE_ID);
    }
  }
}
