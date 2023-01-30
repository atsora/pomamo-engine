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
  /// Migration 601:
  /// </summary>
  [Migration(601)]
  public class InitializeOperationCycleMakesPart: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (InitializeOperationCycleMakesPart).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Full cycles
      Database.ExecuteNonQuery (@"
UPDATE operationcycle
SET operationcyclefull=TRUE
WHERE operationcycleend IS NOT NULL
  AND (operationcyclestatus IS NULL OR operationcyclestatus IN (0, 1));
");
      // Partial cycles
      Database.ExecuteNonQuery (@"
UPDATE operationcycle a
SET operationcyclefull=TRUE
WHERE EXISTS (SELECT 1 FROM config WHERE configkey='Analysis.PartialCycleMakesPart' AND configdescription LIKE '%true%')
  AND operationcyclebegin IS NOT NULL
  AND operationcycleend IS NOT NULL
  AND (operationcyclestatus IS NULL OR operationcyclestatus IN (2))
  AND operationslotid IS NOT NULL
  AND EXISTS (SELECT 1 FROM operationcycle b
              WHERE a.operationcyclebegin<firstorsecond(b.operationcycleend,b.operationcyclebegin)
                AND a.machineid=b.machineid
                AND a.operationslotid=b.operationslotid)
");
      // Operation slot
      Database.ExecuteNonQuery (@"
UPDATE operationslot a
SET operationslottotalcycles=(SELECT COUNT(*) FROM operationcycle b
                              WHERE a.operationslotid=b.operationslotid
                                AND a.machineid=b.machineid
                                AND b.operationcyclefull=TRUE)
");
      Database.ExecuteNonQuery (@"
UPDATE operationslot a
SET operationslotpartialcycles=(SELECT COUNT(*) FROM operationcycle b
                                WHERE a.operationslotid=b.operationslotid
                                  AND a.machineid=b.machineid
                                  AND b.operationcyclefull=FALSE)
");
      Database.ExecuteNonQuery (@"
DELETE FROM config
WHERE configkey='Analysis.PartialCycleMakesPart'
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"
UPDATE operationslot a
SET operationslottotalcycles=(SELECT COUNT(*) FROM operationcycle b
                              WHERE a.operationslotid=b.operationslotid
                                AND a.machineid=b.machineid
                                AND (b.operationcyclestatus IS NULL OR operationcyclestatus IN (0, 1)))
");
      Database.ExecuteNonQuery (@"
UPDATE operationslot a
SET operationslotpartialcycles=(SELECT COUNT(*) FROM operationcycle b
                                WHERE a.operationslotid=b.operationslotid
                                  AND a.machineid=b.machineid
                                  AND b.operationcyclestatus IS NOT NULL
                                  AND b.operationcyclestatus NOT IN (0, 1))
");
    }
  }
}
