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
  /// Migration 700: 
  /// </summary>
  [Migration (700)]
  public class CleanAnalysisConfigKey : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CleanAnalysisConfigKey).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Clean ("SplitCycleSummaryByDay");
      Clean ("SplitIntermediateWorkPieceSummaryByDay");
      Clean ("SplitIntermediateWorkPieceSummaryByShift");
      Clean ("SplitOperationSlotByDay");
      Clean ("SplitOperationSlotByShift");

      // Keep SplitCycleSummaryByShift for the moment
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

    void Clean (string analysisConfigKey)
    {
      Database.ExecuteNonQuery (string.Format (@"
DELETE FROM config
WHERE configkey='Analysis.{0}'
", analysisConfigKey));
    }
  }
}
