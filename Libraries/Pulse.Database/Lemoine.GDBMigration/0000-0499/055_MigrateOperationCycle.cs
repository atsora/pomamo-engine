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
  /// Migration 055: Migrate the data from operationcycleinformation to operationcycle
  /// </summary>
  [Migration(55)]
  public class MigrateOperationCycle: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrateOperationCycle).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Migrate the data
      Database.ExecuteNonQuery (@"
INSERT INTO operationcycle (machineid, operationcyclebegin, operationcycleend)
SELECT machineid, operationcyclebegin,
CASE WHEN (nextbegin IS NULL) OR (nextend <= nextbegin)
  THEN nextend else NULL END AS operationcycleend
FROM (
select o1.machineid AS machineid, o1.operationcycleapplicationdatetime AS operationcyclebegin,
(SELECT MIN (o2.operationcycleapplicationdatetime)
FROM operationcycleinformation o2
WHERE o2.machineid=o1.machineid
AND o2.operationcycleapplicationdatetime>o1.operationcycleapplicationdatetime
AND o2.operationcyclebegin) AS nextbegin,
(SELECT MIN (o3.operationcycleapplicationdatetime)
FROM operationcycleinformation o3
WHERE o3.machineid=o1.machineid
AND o3.operationcycleapplicationdatetime>o1.operationcycleapplicationdatetime
AND o3.operationcycleend) AS nextend
FROM operationcycleinformation o1
WHERE o1.operationcyclebegin
) subrequest");
      Database.ExecuteNonQuery (@"
INSERT INTO operationcycle (machineid, operationcycleend)
SELECT machineid, operationcycleend
FROM (
select o1.machineid AS machineid, o1.operationcycleapplicationdatetime AS operationcycleend,
(SELECT MAX (o2.operationcycleapplicationdatetime)
FROM operationcycleinformation o2
WHERE o2.machineid=o1.machineid
AND o2.operationcycleapplicationdatetime<o1.operationcycleapplicationdatetime
AND o2.operationcyclebegin) AS previousbegin,
(SELECT MAX (o3.operationcycleapplicationdatetime)
FROM operationcycleinformation o3
WHERE o3.machineid=o1.machineid
AND o3.operationcycleapplicationdatetime<o1.operationcycleapplicationdatetime
AND o3.operationcycleend) AS previousend
FROM operationcycleinformation o1
WHERE o1.operationcycleend
) subrequest
WHERE (previousend IS NULL) OR (previousbegin<previousend)");
      
      // Remove the old data
      Database.ExecuteNonQuery (@"
DELETE FROM modification
WHERE modificationreferencedtable='OperationCycleInformation'");
      Database.ExecuteNonQuery (@"
DELETE FROM operationcycleinformation");
      
      // Remove the columns
      // operationcyclebegin and operationcycleend
      Database.ExecuteNonQuery (@"ALTER TABLE operationcycleinformation
DROP COLUMN operationcyclebegin CASCADE;");
      Database.ExecuteNonQuery (@"ALTER TABLE operationcycleinformation
DROP COLUMN operationcycleend CASCADE;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
