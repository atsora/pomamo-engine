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
  /// Migration 275:
  /// </summary>
  [Migration (275)]
  public class AddIntermediateWorkPieceSummaryView : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddIntermediateWorkPieceSummaryView).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
        return;
      }

      Database.RemoveTable (TableName.INTERMEDIATE_WORK_PIECE_SUMMARY);
      Database.ExecuteNonQuery (string.Format (@"
CREATE OR REPLACE VIEW {0} AS
SELECT MIN(iwpbymachinesummaryid) AS intermediateworkpiecesummaryid,
  intermediateworkpieceid, componentid, workorderid,
  SUM(iwpbymachinesummarycounted) AS {0}counted, SUM(iwpbymachinesummarycorrected) AS {0}corrected,
  SUM(iwpbymachinesummarychecked) AS {0}checked, SUM(iwpbymachinesummaryscrapped) AS {0}scrapped,
  lineid, iwpbymachinesummaryday AS {0}day, shiftid
FROM iwpbymachinesummary
GROUP BY intermediateworkpieceid, componentid, workorderid, lineid, iwpbymachinesummaryday, shiftid",
                                               TableName.INTERMEDIATE_WORK_PIECE_SUMMARY));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
    }
  }
}
