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
  /// Migration 151: Add two columns to cycledurationsummary:
  /// <item>a reference to the shift</item>
  /// <item>the number of partial cycles</item>
  /// 
  /// Add also an index to the table
  /// </summary>
  [Migration(151)]
  public class AddCycleDurationSummaryShiftPartialColumns: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCycleDurationSummaryShiftPartialColumns).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.CYCLE_DURATION_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleDurationSummary
        return;
      }

      Database.AddColumn (TableName.CYCLE_DURATION_SUMMARY,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.CYCLE_DURATION_SUMMARY,
                          new Column (TableName.CYCLE_DURATION_SUMMARY + "partial", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.GenerateForeignKey (TableName.CYCLE_DURATION_SUMMARY, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddConstraintPositive (TableName.CYCLE_DURATION_SUMMARY, TableName.CYCLE_DURATION_SUMMARY + "partial");
      
      AddUniqueConstraint (TableName.CYCLE_DURATION_SUMMARY,
                           ColumnName.MACHINE_ID,
                           TableName.CYCLE_DURATION_SUMMARY + "day",
                           ColumnName.SHIFT_ID,
                           ColumnName.OPERATION_ID,
                           ColumnName.COMPONENT_ID,
                           ColumnName.WORK_ORDER_ID,
                           TableName.CYCLE_DURATION_SUMMARY + "offset");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Now part of Lemoine.Plugin.CycleDurationSummary
    }
  }
}
