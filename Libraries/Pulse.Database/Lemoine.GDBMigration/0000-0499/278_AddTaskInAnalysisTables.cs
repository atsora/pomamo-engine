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
    // Keep the options until all customers were upgraded to version >= 19.0.0
    // And after version >= 19.0.0 was installed at new customers
    static readonly string USE_DEPRECATED_TASK_KEY = "Migration.UseDeprecatedTask";
    static readonly bool USE_DEPRECATED_TASK_DEFAULT = false;

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
      CycleDurationSummaryDown ();
      IntermediateWorkPieceByMachineSummaryDown ();
      OperationSlotDown ();
      WorkOrderSlotDown ();
    }

    void OperationSlotUp ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)) {
        Database.AddColumn (TableName.OPERATION_SLOT,
                            new Column (ColumnName.TASK_ID, DbType.Int32, ColumnProperty.Null));
        Database.AddColumn (TableName.OPERATION_SLOT,
                            new Column (TableName.OPERATION_SLOT + "autotask", DbType.Boolean, ColumnProperty.Null));
      }
      else {
        Database.AddColumn (TableName.OPERATION_SLOT,
                            new Column (ColumnName.MANUFACTURING_ORDER_ID, DbType.Int32, ColumnProperty.Null));
        Database.AddColumn (TableName.OPERATION_SLOT,
                            new Column (TableName.OPERATION_SLOT + "automanuforder", DbType.Boolean, ColumnProperty.Null));
      }
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
      if (Database.ColumnExists (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "automanuforder")) {
        Database.RemoveColumn (TableName.OPERATION_SLOT,
                               TableName.OPERATION_SLOT + "automanuforder");
      }
      if (Database.ColumnExists (TableName.OPERATION_SLOT, ColumnName.MANUFACTURING_ORDER_ID)) {
        Database.RemoveColumn (TableName.OPERATION_SLOT,
                               ColumnName.MANUFACTURING_ORDER_ID);
      }

      // Keep the next lines until all customers were upgraded to version >= 19.0.0
      // And after version >= 19.0.0 was installed at new customers
      if (Database.ColumnExists (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "autotask")) {
        Database.RemoveColumn (TableName.OPERATION_SLOT,
                               TableName.OPERATION_SLOT + "autotask");
      }
      if (Database.ColumnExists (TableName.OPERATION_SLOT, ColumnName.TASK_ID)) {
        Database.RemoveColumn (TableName.OPERATION_SLOT,
                               ColumnName.TASK_ID);
      }
    }

    void WorkOrderSlotUp ()
    {
      var taskIdColumn =
        Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)
        ? ColumnName.TASK_ID
        : ColumnName.MANUFACTURING_ORDER_ID;
      Database.AddColumn (TableName.WORK_ORDER_SLOT,
                          new Column (taskIdColumn, DbType.Int32, ColumnProperty.Null));
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
      if (Database.ColumnExists (TableName.WORK_ORDER_SLOT, ColumnName.MANUFACTURING_ORDER_ID)) {
        Database.RemoveColumn (TableName.WORK_ORDER_SLOT,
                               ColumnName.MANUFACTURING_ORDER_ID);
      }

      // Keep the next lines until all customers were upgraded to version >= 19.0.0
      // And after version >= 19.0.0 was installed at new customers
      if (Database.ColumnExists (TableName.WORK_ORDER_SLOT, ColumnName.TASK_ID)) {
        Database.RemoveColumn (TableName.WORK_ORDER_SLOT,
                               ColumnName.TASK_ID);
      }
    }

    void IntermediateWorkPieceByMachineSummaryUp ()
    {
      if (!Database.TableExists (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
        return;
      }

      RemoveUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY);

      Database.AddColumn (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                          new Column (ColumnName.MANUFACTURING_ORDER_ID, DbType.Int32, ColumnProperty.Null));
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
                               ColumnName.MANUFACTURING_ORDER_ID,
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
                          new Column (ColumnName.MANUFACTURING_ORDER_ID, DbType.Int32, ColumnProperty.Null));
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
                               ColumnName.MANUFACTURING_ORDER_ID});
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
                          new Column (ColumnName.MANUFACTURING_ORDER_ID, DbType.Int32, ColumnProperty.Null));
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
                               ColumnName.MANUFACTURING_ORDER_ID,
                               TableName.CYCLE_DURATION_SUMMARY + "offset"});
    }

    void CycleDurationSummaryDown ()
    {
      // Now part of Lemoine.Plugin.CycleDurationSummary
    }
  }
}
