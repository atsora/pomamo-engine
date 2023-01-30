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
  /// Migration 142: Fill the CycleDurationSummary analysis table with the current data
  /// 
  /// Do not do it any more. A custom action can be added later to the plugin if necessary
  /// </summary>
  [Migration(142)]
  public class MigrationCycleDurationSummary: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrationCycleDurationSummary).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.CYCLE_DURATION_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleDurationSummary
        return;
      }

      // Remove first a constraint that should not exist
      RemoveConstraint (TableName.CYCLE_DURATION_SUMMARY, TableName.CYCLE_DURATION_SUMMARY + "offset_positive");
     
      /* // To be done later in a custom action of the plugin if required
      Database.ExecuteNonQuery (@"
WITH operationcycle2 AS (
  SELECT machineid, ConvertUtcToDay (operationcycleend) AS operationcycleday, CAST (ROUND (operationcycleoffsetduration) AS integer) AS operationcycleoffsetduration,
    operationid, componentid, workorderid  
  FROM operationcycle NATURAL JOIN operationslot
  WHERE operationslotid IS NOT NULL
    AND operationcycleend IS NOT NULL
    AND operationcycleoffsetduration IS NOT NULL
)
INSERT INTO cycledurationsummary (machineid, cycledurationsummaryday, cycledurationsummaryoffset, operationid, componentid, workorderid, cycledurationsummarynumber)
SELECT machineid, operationcycleday, operationcycleoffsetduration, operationid, componentid, workorderid, COUNT(*) AS count
FROM operationcycle2
GROUP BY machineid, operationcycleday, operationcycleoffsetduration, operationid, componentid, workorderid");
*/
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (!Database.TableExists (TableName.CYCLE_DURATION_SUMMARY)) {
        // Now part of Lemoine.Plugin.CycleDurationSummary
        return;
      }

      Database.ExecuteNonQuery (@"TRUNCATE cycledurationsummary RESTART IDENTITY");
    }
  }
}
