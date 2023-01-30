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
  /// Migration 122: Migrate the data from autosequence to sequenceslot table
  /// </summary>
  [Migration(122)]
  public class MigrateSequenceSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrateSequenceSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Clean first the autosequence table (else some constraints may fail)
      Database.ExecuteNonQuery (@"WITH duplicate AS (
     SELECT machinemoduleid, autosequenceactivitybegin, MAX(autosequenceactivityend) AS maxend
     FROM autosequence
     GROUP BY machinemoduleid, autosequenceactivitybegin
     HAVING count (*)> 1)
DELETE FROM autosequence
WHERE (machinemoduleid, autosequenceactivitybegin) IN (SELECT machinemoduleid, autosequenceactivitybegin FROM duplicate)
  AND autosequenceactivityend < (SELECT maxend FROM duplicate
                    WHERE duplicate.machinemoduleid=autosequence.machinemoduleid
                      AND duplicate.autosequenceactivitybegin=autosequence.autosequenceactivitybegin);
");
      Database.ExecuteNonQuery (@"WITH duplicate AS (
     SELECT machinemoduleid, autosequenceactivitybegin, MIN(autosequenceid) AS minid
     FROM autosequence
     GROUP BY machinemoduleid, autosequenceactivitybegin
     HAVING count (*)> 1)
DELETE FROM autosequence
WHERE (machinemoduleid, autosequenceactivitybegin) IN (SELECT machinemoduleid, autosequenceactivitybegin FROM duplicate)
  AND autosequenceid > (SELECT minid FROM duplicate
                    WHERE duplicate.machinemoduleid=autosequence.machinemoduleid
                      AND duplicate.autosequenceactivitybegin=autosequence.autosequenceactivitybegin);");
      
      Database.ExecuteNonQuery (@"INSERT INTO sequenceslot (machinemoduleid, sequenceslotbegin, sequenceslotend, sequenceid, sequenceslotanalysisstatusid, sequenceslotanalysisdatetime) 
SELECT machinemoduleid, autosequenceactivitybegin, autosequenceactivityend, sequenceid, 2, autosequenceanalysis
FROM autosequence
WHERE autosequenceactivitybegin is not null");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
