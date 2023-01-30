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
  /// Migration 070: add OperationCycleDeliverablePiece table
  /// </summary>
  [Migration(70)]
  public class OperationCycleDeliverablePiece: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycleDeliverablePiece).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                         new Column(ColumnName.OPERATION_CYCLE_DELIVERABLE_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column(ColumnName.OPERATION_CYCLE_DELIVERABLE_PIECE_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.DELIVERABLE_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.OPERATION_CYCLE_ID, DbType.Int32, ColumnProperty.NotNull));
      

      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                                   ColumnName.DELIVERABLE_PIECE_ID,
                                   TableName.DELIVERABLE_PIECE, ColumnName.DELIVERABLE_PIECE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                                   ColumnName.OPERATION_CYCLE_ID,
                                   TableName.OPERATION_CYCLE,
                                   ColumnName.OPERATION_CYCLE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      AddNamedUniqueConstraint ("operationcycledeliverablepiece_SecondaryKey",
                             TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                             new string [] { ColumnName.DELIVERABLE_PIECE_ID,
                               ColumnName.OPERATION_CYCLE_ID});

      AddIndex (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                ColumnName.DELIVERABLE_PIECE_ID);
      AddIndex (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                ColumnName.OPERATION_CYCLE_ID);

      /*
      Database.ExecuteNonQuery ("CREATE INDEX " + TableName.OPERATION_CYCLE_DELIVERABLE_PIECE +
                                "_" + ColumnName.DELIVERABLE_PIECE_ID +
                                " ON " + TableName.OPERATION_CYCLE_DELIVERABLE_PIECE +
                                " USING btree (" + ColumnName.DELIVERABLE_PIECE_ID + ") WHERE " +
                                ColumnName.DELIVERABLE_PIECE_ID + " IS NOT NULL;");
      
      Database.ExecuteNonQuery ("CREATE INDEX " + TableName.OPERATION_CYCLE_DELIVERABLE_PIECE +
                                "_" + ColumnName.OPERATION_CYCLE_ID +
                                " ON " + TableName.OPERATION_CYCLE_DELIVERABLE_PIECE +
                                " USING btree (" + ColumnName.OPERATION_CYCLE_ID + ") WHERE " +
                                ColumnName.OPERATION_CYCLE_ID + " IS NOT NULL;");
       */
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE);
    }
  }
}
