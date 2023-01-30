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
  /// Migration 290:
  /// </summary>
  [Migration(290)]
  public class MakeCycleCountSummaryFlexible: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MakeCycleCountSummaryFlexible).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.CYCLE_COUNT_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleCountSummary
        return;
      }

      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.CYCLE_COUNT_SUMMARY,
                                               TableName.CYCLE_COUNT_SUMMARY + ColumnName.DAY));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
