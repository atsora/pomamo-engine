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
  /// Migration 500: if the maximum duration is null, the existing unique constraint is not considered
  /// </summary>
  [Migration(500)]
  public class FixUniqueConstraintMachineModeDefaultReason: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixUniqueConstraintMachineModeDefaultReason).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Fix the table first if needed
      Database.ExecuteNonQuery (@"
WITH a AS (SELECT machinemodeid, machineobservationstateid, count(*), min(machinemodedefaultreasonid) AS id
FROM machinemodedefaultreason
WHERE defaultreasonmaximumduration IS NULL
  AND includemachinefilterid IS NULL
GROUP BY machinemodeid, machineobservationstateid
HAVING count(*) > 1
)
DELETE FROM machinemodedefaultreason
USING a
WHERE machinemodedefaultreason.machinemodedefaultreasonid=a.id");
      
      RemoveIndex (TableName.MACHINE_MODE_DEFAULT_REASON,
                   ColumnName.MACHINE_MODE_ID,
                   ColumnName.MACHINE_OBSERVATION_STATE_ID);
      RemoveUniqueConstraint (TableName.MACHINE_MODE_DEFAULT_REASON,
                              ColumnName.MACHINE_MODE_ID,
                              ColumnName.MACHINE_OBSERVATION_STATE_ID,
                              "defaultreasonmaximumduration");
      AddUniqueIndex (TableName.MACHINE_MODE_DEFAULT_REASON,
                      ColumnName.MACHINE_MODE_ID,
                      ColumnName.MACHINE_OBSERVATION_STATE_ID,
                      "COALESCE(defaultreasonmaximumduration,-1)",
                      "COALESCE(includemachinefilterid, 0)");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.MACHINE_MODE_DEFAULT_REASON,
                   ColumnName.MACHINE_MODE_ID,
                   ColumnName.MACHINE_OBSERVATION_STATE_ID,
                   "COALESCE(defaultreasonmaximumduration,-1)",
                   "COALESCE(includemachinefilterid, 0)");
      AddUniqueConstraint (TableName.MACHINE_MODE_DEFAULT_REASON,
                           ColumnName.MACHINE_MODE_ID,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           "defaultreasonmaximumduration");
      AddIndex (TableName.MACHINE_MODE_DEFAULT_REASON,
                ColumnName.MACHINE_MODE_ID,
                ColumnName.MACHINE_OBSERVATION_STATE_ID);
    }
  }
}
