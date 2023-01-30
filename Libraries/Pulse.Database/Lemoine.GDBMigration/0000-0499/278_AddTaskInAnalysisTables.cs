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
  /// Migration 278:
  /// </summary>
  [Migration (278)]
  public class AddTaskInAnalysisTables : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddTaskInAnalysisTables).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      OperationSlotUp ();
      WorkOrderSlotUp ();
      IntermediateWorkPieceByMachineSummaryUp ();
      CycleCountSummaryUp ();
      CycleDurationSummaryUp ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      CycleCountSummaryDown ();
      CycleCountSummaryDown ();
      IntermediateWorkPieceByMachineSummaryDown ();
      OperationSlotDown ();
      WorkOrderSlotDown ();
    }

    void OperationSlotUp ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (ColumnName.TASK_ID, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "autotask", DbType.Boolean, ColumnProperty.Null));
      // Note: this is not possible to add a foreign key to a view
      // So next constraint can't be written
      /*
      Database.GenerateForeignKey (TableName.OPERATION_SLOT, ColumnName.TASK_ID,
                                   TableName.TASK, ColumnName.TASK_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
       */
    }

    void OperationSlotDown ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             TableName.OPERATION_SLOT + "autotask");
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             ColumnName.TASK_ID);
    }

    void WorkOrderSlotUp ()
    {
      Database.AddColumn (TableName.WORK_ORDER_SLOT,
                          new Column (ColumnName.TASK_ID, DbType.Int32, ColumnProperty.Null));
      // Note: this is not possible to add a foreign key to a view
      // So next constraint can't be written
      /*
      Database.GenerateForeignKey (TableName.WORK_ORDER_SLOT, ColumnName.TASK_ID,
                                   TableName.TASK, ColumnName.TASK_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
       */
    }

    void WorkOrderSlotDown ()
    {
      Database.RemoveColumn (TableName.WORK_ORDER_SLOT,
                             ColumnName.TASK_ID);
    }

    void IntermediateWorkPieceByMachineSummaryUp ()
    {
      if (!Database.TableExists (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
        return;
      }

      RemoveUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY);

      Database.AddColumn (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                          new Column (ColumnName.TASK_ID, DbType.Int32, ColumnProperty.Null));
      // Note: this is not possible to add a foreign key to a view
      // So next constraint can't be written
      /*
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.TASK_ID,
                                   TableName.TASK, ColumnName.TASK_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
       */
      AddNamedUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY + "_unique",
                             TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                             new string[] {
                               ColumnName.TASK_ID,
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

    void CycleCountSummaryUp ()
    {
      if (!Database.TableExists (TableName.CYCLE_COUNT_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleCountSummary
        return;
      }

      RemoveUniqueConstraint (TableName.CYCLE_COUNT_SUMMARY);

      Database.AddColumn (TableName.CYCLE_COUNT_SUMMARY,
                          new Column (ColumnName.TASK_ID, DbType.Int32, ColumnProperty.Null));
      // Note: this is not possible to add a foreign key to a view
      // So next constraint can't be written
      /*
      Database.GenerateForeignKey (TableName.CYCLE_COUNT_SUMMARY, ColumnName.TASK_ID,
                                   TableName.TASK, ColumnName.TASK_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
       */
      AddNamedUniqueConstraint (TableName.CYCLE_COUNT_SUMMARY + "_unique",
                             TableName.CYCLE_COUNT_SUMMARY,
                             new string[] {ColumnName.MACHINE_ID,
                               TableName.CYCLE_COUNT_SUMMARY + "day",
                               ColumnName.SHIFT_ID,
                               ColumnName.OPERATION_ID,
                               ColumnName.COMPONENT_ID,
                               ColumnName.WORK_ORDER_ID,
                               ColumnName.LINE_ID,
                               ColumnName.TASK_ID});
    }

    void CycleCountSummaryDown ()
    {
      // Now part of Lemoine.Plugin.CycleCountSummary
    }

    void CycleDurationSummaryUp ()
    {
      if (!Database.TableExists (TableName.CYCLE_DURATION_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleDurationSummary
        return;
      }

      RemoveUniqueConstraint (TableName.CYCLE_DURATION_SUMMARY);

      Database.AddColumn (TableName.CYCLE_DURATION_SUMMARY,
                          new Column (ColumnName.TASK_ID, DbType.Int32, ColumnProperty.Null));
      // Note: this is not possible to add a foreign key to a view
      // So next constraint can't be written
      /*
      Database.GenerateForeignKey (TableName.CYCLE_DURATION_SUMMARY, ColumnName.LINE_ID,
                                   TableName.TASK, ColumnName.TASK_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
       */

      AddNamedUniqueConstraint (TableName.CYCLE_DURATION_SUMMARY + "_unique",
                             TableName.CYCLE_DURATION_SUMMARY,
                             new string[] {ColumnName.MACHINE_ID,
                               TableName.CYCLE_DURATION_SUMMARY + "day",
                               ColumnName.SHIFT_ID,
                               ColumnName.OPERATION_ID,
                               ColumnName.COMPONENT_ID,
                               ColumnName.WORK_ORDER_ID,
                               ColumnName.LINE_ID,
                               ColumnName.TASK_ID,
                               TableName.CYCLE_DURATION_SUMMARY + "offset"});
    }

    void CycleDurationSummaryDown ()
    {
      // Now part of Lemoine.Plugin.CycleDurationSummary
    }
  }
}
