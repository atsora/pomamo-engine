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
  /// Migration 326:
  /// </summary>
  [Migration(326)]
  public class AddMachineStateTemplateSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineStateTemplateSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Use of the function gen_random_uuid() in pgcrypto
      Database.ExecuteNonQuery (@"CREATE EXTENSION IF NOT EXISTS pgcrypto");

      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW machinestatetemplateslot AS
SELECT gen_random_uuid() AS machinestatetemplateslotuuid, machineid, machinestatetemplateid, UNNEST(ranges) AS machinestatetemplateslotdatetimerange
FROM (
  SELECT machineid, machinestatetemplateid, merge_ranges (observationstateslotdatetimerange) AS ranges
  FROM observationstateslot
  GROUP BY machineid, machinestatetemplateid
) a;
");
      
      // Note: the same request with a CTE is not so efficient because the CTE prevents
      //       from using the partitioning key machineid
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"
DROP VIEW IF EXISTS machinestatetemplateslot");
    }
  }
}
