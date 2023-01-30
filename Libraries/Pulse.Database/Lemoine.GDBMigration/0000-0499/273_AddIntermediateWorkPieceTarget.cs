// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration.Properties
{
  /// <summary>
  /// Migration 273:
  /// </summary>
  [Migration(273)]
  public class AddIntermediateWorkPieceTarget: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIntermediateWorkPieceTarget).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CreateTableIwpTarget();
      MigrateSummaryToTarget();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DeleteTableIwpTarget();
    }
    
    void CreateTableIwpTarget()
    {
      Database.AddTable (TableName.INTERMEDIATE_WORK_PIECE_TARGET,
                         new Column (TableName.INTERMEDIATE_WORK_PIECE_TARGET + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.INTERMEDIATE_WORK_PIECE_TARGET + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.INTERMEDIATE_WORK_PIECE_TARGET + "day", DbType.Date, ColumnProperty.Null),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.INTERMEDIATE_WORK_PIECE_TARGET + "number", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_TARGET, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                   TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_TARGET, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_TARGET, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_TARGET, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_TARGET, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      AddNamedUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE_TARGET + "_unique",
                             TableName.INTERMEDIATE_WORK_PIECE_TARGET,
                             new string[] {ColumnName.LINE_ID,
                               ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                               TableName.INTERMEDIATE_WORK_PIECE_TARGET + "day",
                               ColumnName.SHIFT_ID,
                               ColumnName.WORK_ORDER_ID,
                               ColumnName.COMPONENT_ID
                             });
      
      AddIndex (TableName.INTERMEDIATE_WORK_PIECE_TARGET, ColumnName.INTERMEDIATE_WORK_PIECE_ID);
      AddIndex (TableName.INTERMEDIATE_WORK_PIECE_TARGET,
                TableName.INTERMEDIATE_WORK_PIECE_TARGET + "day",
                ColumnName.SHIFT_ID);
    }
    
    void MigrateSummaryToTarget()
    {
      if (!Database.TableExists (TableName.INTERMEDIATE_WORK_PIECE_SUMMARY)) {
        // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
        return;
      }

      Database.ExecuteNonQuery (@"
INSERT INTO intermediateworkpiecetarget (intermediateworkpieceid, componentid, workorderid, lineid, shiftid,
  intermediateworkpiecetargetday, intermediateworkpiecetargetnumber)
SELECT intermediateworkpieceid, componentid, workorderid, lineid, shiftid, intermediateworkpiecesummaryday,
  intermediateworkpiecesummarytargeted
FROM intermediateworkpiecesummary
WHERE intermediateworkpiecesummarytargeted IS NOT NULL");
    }
    
    void DeleteTableIwpTarget()
    {
      Database.RemoveTable (TableName.INTERMEDIATE_WORK_PIECE_TARGET);
    }
  }
}
