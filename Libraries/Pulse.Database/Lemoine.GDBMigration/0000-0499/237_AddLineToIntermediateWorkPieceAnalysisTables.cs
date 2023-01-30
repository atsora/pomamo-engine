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
  /// Migration 237: add a reference to the line to the following tables:
  /// <item>intermediateworkpiecesummary</item>
  /// <item>iwpbymachinesummary</item>
  /// <item>cyclecountsummary</item>
  /// <item>cycledurationsummary</item>
  /// </summary>
  [Migration (237)]
  public class AddLineToIntermediateWorkPieceAnalysisTables : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddLineToIntermediateWorkPieceAnalysisTables).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      IntermediateWorkPieceSummaryUp ();
      IntermediateWorkPieceByMachineSummaryUp ();
      CycleCountSummaryUp ();
      CycleDurationSummaryUp ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      CycleDurationSummaryDown ();
      CycleCountSummaryDown ();
      IntermediateWorkPieceByMachineSummaryDown ();
      IntermediateWorkPieceSummaryDown ();
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

      RemoveConstraint (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                        TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY + "unique");

      Database.AddColumn (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                          new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);

      AddNamedUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY + "_unique",
                             TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY,
                             new string[] {ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                               ColumnName.COMPONENT_ID,
                               ColumnName.WORK_ORDER_ID,
                               ColumnName.LINE_ID,
                               ColumnName.MACHINE_ID});
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

      RemoveUniqueConstraint (TableName.CYCLE_COUNT_SUMMARY,
                              ColumnName.MACHINE_ID,
                              TableName.CYCLE_COUNT_SUMMARY + "day",
                              ColumnName.SHIFT_ID,
                              ColumnName.OPERATION_ID,
                              ColumnName.COMPONENT_ID,
                              ColumnName.WORK_ORDER_ID);

      Database.AddColumn (TableName.CYCLE_COUNT_SUMMARY,
                          new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.CYCLE_COUNT_SUMMARY, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);

      AddNamedUniqueConstraint (TableName.CYCLE_COUNT_SUMMARY + "_unique",
                             TableName.CYCLE_COUNT_SUMMARY,
                             new string[] {ColumnName.MACHINE_ID,
                               TableName.CYCLE_COUNT_SUMMARY + "day",
                               ColumnName.SHIFT_ID,
                               ColumnName.OPERATION_ID,
                               ColumnName.COMPONENT_ID,
                               ColumnName.WORK_ORDER_ID,
                               ColumnName.LINE_ID});
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

      RemoveUniqueConstraint (TableName.CYCLE_DURATION_SUMMARY,
                              ColumnName.MACHINE_ID,
                              TableName.CYCLE_DURATION_SUMMARY + "day",
                              ColumnName.SHIFT_ID,
                              ColumnName.OPERATION_ID,
                              ColumnName.COMPONENT_ID,
                              ColumnName.WORK_ORDER_ID,
                              TableName.CYCLE_DURATION_SUMMARY + "offset");

      Database.AddColumn (TableName.CYCLE_DURATION_SUMMARY,
                          new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.CYCLE_DURATION_SUMMARY, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);

      AddNamedUniqueConstraint (TableName.CYCLE_DURATION_SUMMARY + "_unique",
                             TableName.CYCLE_DURATION_SUMMARY,
                             new string[] {ColumnName.MACHINE_ID,
                               TableName.CYCLE_DURATION_SUMMARY + "day",
                               ColumnName.SHIFT_ID,
                               ColumnName.OPERATION_ID,
                               ColumnName.COMPONENT_ID,
                               ColumnName.WORK_ORDER_ID,
                               ColumnName.LINE_ID,
                               TableName.CYCLE_DURATION_SUMMARY + "offset"});
    }

    void CycleDurationSummaryDown ()
    {
      // Now part of Lemoine.Plugin.CycleDurationSummary
    }
  }
}
