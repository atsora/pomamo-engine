// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 631: 
  /// </summary>
  [Migration (631)]
  public class CycleCountSummaryDayNotNull : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CycleCountSummaryDayNotNull).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.CYCLE_COUNT_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleCountSummary
        return;
      }

      Database.ExecuteNonQuery (@"
DELETE FROM cyclecountsummary
WHERE cyclecountsummaryday IS NULL;
");
      SetNotNull (TableName.CYCLE_COUNT_SUMMARY, TableName.CYCLE_COUNT_SUMMARY + "day");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
