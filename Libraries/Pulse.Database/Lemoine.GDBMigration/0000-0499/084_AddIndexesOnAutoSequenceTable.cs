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
  /// Migration 084: Add indexes on autosequence table
  /// </summary>
  [Migration(84)]
  public class AddIndexesOnAutoSequenceTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexesOnAutoSequenceTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Because they are removed in migration 123, do not insert the indexes below
      /*
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
       */
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS autosequence_for_purge_idx");
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS autosequence_analysis_not_completed_idx");
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS autosequence_no_analysis_idx");
    }
  }
}
