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
  /// Migration 123: Migrate the table autosequence for the new architecture
  /// <item>clean the database</item>
  /// <item>remove the deprecated columns</item>
  /// </summary>
  [Migration(123)]
  public class MigrateAutoSequence: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrateAutoSequence).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS autosequence_for_purge_idx");
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS autosequence_analysis_not_completed_idx");
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS autosequence_no_analysis_idx");
      
      Database.ExecuteNonQuery (@"TRUNCATE autosequence");
      Database.RemoveColumn (TableName.AUTO_SEQUENCE, TableName.AUTO_SEQUENCE + "analysis");
      Database.RemoveColumn (TableName.AUTO_SEQUENCE, TableName.AUTO_SEQUENCE + "activityend");
      Database.RemoveColumn (TableName.AUTO_SEQUENCE, TableName.AUTO_SEQUENCE + "activitybegin");
      
      AddUniqueConstraint (TableName.AUTO_SEQUENCE, ColumnName.MACHINE_MODULE_ID, TableName.AUTO_SEQUENCE + "begin"); // Also used as an index
      AddUniqueConstraint (TableName.AUTO_SEQUENCE, ColumnName.MACHINE_MODULE_ID, TableName.AUTO_SEQUENCE + "end"); // Also used as an index
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUniqueConstraint (TableName.AUTO_SEQUENCE, ColumnName.MACHINE_MODULE_ID, TableName.AUTO_SEQUENCE + "begin"); // Also used as an index
      RemoveUniqueConstraint (TableName.AUTO_SEQUENCE, ColumnName.MACHINE_MODULE_ID, TableName.AUTO_SEQUENCE + "end"); // Also used as an index
      
      Database.AddColumn (TableName.AUTO_SEQUENCE,
                          new Column (TableName.AUTO_SEQUENCE + "activitybegin", DbType.DateTime));
      Database.AddColumn (TableName.AUTO_SEQUENCE,
                          new Column (TableName.AUTO_SEQUENCE + "activityend", DbType.DateTime));
      Database.AddColumn (TableName.AUTO_SEQUENCE,
                          new Column (TableName.AUTO_SEQUENCE + "analysis", DbType.DateTime));
      
      // indexes
      Database.ExecuteNonQuery (@"CREATE INDEX autosequence_for_purge_idx
  ON autosequence
  USING btree (machinemoduleid )
  WHERE autosequenceend IS NOT NULL
    AND (autosequenceanalysis IS NOT NULL AND autosequenceend <= autosequenceanalysis AND autosequenceactivityend IS NOT NULL
         AND (autosequenceend < autosequenceactivityend AND autosequencebegin < autosequenceactivityend
              OR autosequenceend <= autosequencebegin AND autosequenceactivityend <= autosequencebegin)
         OR autosequenceend <= autosequencebegin AND (autosequenceanalysis IS NULL OR autosequenceanalysis < autosequenceend));");
      Database.ExecuteNonQuery (@"CREATE INDEX autosequence_analysis_not_completed_idx
  ON autosequence
  USING btree (machinemoduleid )
  WHERE autosequenceanalysis IS NULL OR autosequenceend IS NULL OR autosequenceanalysis < autosequenceend;");
      Database.ExecuteNonQuery (@"CREATE INDEX autosequence_no_analysis_idx
  ON autosequence
  USING btree
  (machinemoduleid )
  WHERE autosequenceanalysis IS NULL;");
      
      Database.ExecuteNonQuery (@"INSERT INTO autosequence (machinemoduleid, sequenceid, autosequencebegin, autosequenceend, autosequenceactivitybegin, autosequenceactivityend, autosequenceanalysis)
SELECT machinemoduleid, sequenceid, sequenceslotbegin, sequenceslotend, sequenceslotbegin, sequenceslotend, sequenceslotend
FROM sequenceslot");
    }
  }
}
