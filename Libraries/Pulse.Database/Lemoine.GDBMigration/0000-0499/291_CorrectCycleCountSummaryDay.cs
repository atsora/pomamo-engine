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
  /// Migration 291:
  /// </summary>
  [Migration(291)]
  public class CorrectCycleCountSummaryDay: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CorrectCycleCountSummaryDay).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.CYCLE_COUNT_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleCountSummary
        ConvertToDate (TableName.CYCLE_COUNT_SUMMARY, TableName.CYCLE_COUNT_SUMMARY + "day");
      }

      if (Database.TableExists (TableName.CYCLE_DURATION_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleDurationSummary
        ConvertToDate (TableName.CYCLE_DURATION_SUMMARY, TableName.CYCLE_DURATION_SUMMARY + "day");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void ConvertToDate (string tableName, string columnName)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} SET DATA TYPE date;",
                                               tableName, columnName));
    }
  }
}
