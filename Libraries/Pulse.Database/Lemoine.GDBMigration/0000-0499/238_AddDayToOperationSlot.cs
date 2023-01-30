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
  /// Migration 238: add a day column to the following tables:
  /// <item>operationslot</item>
  /// 
  /// Add the shiftid and day column to the following tables:
  /// <item>intermediateworkpiecesummary</item>
  /// <item>iwpbymachinesummary</item>
  /// </summary>
  [Migration (238)]
  public class AddDayToOperationSlot : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddDayToOperationSlot).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      OperationSlotUp ();
      IntermediateWorkPieceSummaryUp ();
      IntermediateWorkPieceByMachineSummaryUp ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      IntermediateWorkPieceByMachineSummaryDown ();
      IntermediateWorkPieceSummaryDown ();
      OperationSlotDown ();
    }

    void OperationSlotUp ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "day", DbType.Date, ColumnProperty.Null));
      AddIndex (TableName.OPERATION_SLOT,
                TableName.OPERATION_SLOT + "day",
                ColumnName.SHIFT_ID);
    }

    void OperationSlotDown ()
    {
      RemoveIndex (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "day", ColumnName.SHIFT_ID);
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             TableName.OPERATION_SLOT + "day");
    }

    void IntermediateWorkPieceSummaryUp ()
    {
      // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
    }

    void IntermediateWorkPieceSummaryDown ()
    {
      // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
    }

    void IntermediateWorkPieceByMachineSummaryUp ()
    {
      if (!Database.TableExists (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
        return;
      }

      RemoveUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY);

      Database.AddColumn (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                          new Column (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY + "day", DbType.Date, ColumnProperty.Null));
      Database.AddColumn (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY + "day",
                ColumnName.SHIFT_ID);

      AddNamedUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY + "_unique",
                             TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                             new string[] {
                               ColumnName.LINE_ID,
                               ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                               TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY + "day",
                               ColumnName.SHIFT_ID,
                               ColumnName.MACHINE_ID,
                               ColumnName.WORK_ORDER_ID,
                               ColumnName.COMPONENT_ID
                             });
    }

    void IntermediateWorkPieceByMachineSummaryDown ()
    {
      // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
    }
  }
}
