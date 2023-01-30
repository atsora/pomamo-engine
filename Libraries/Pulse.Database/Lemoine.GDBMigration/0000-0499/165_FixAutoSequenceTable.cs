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
  /// Migration 165: Fix the autosequence table, where there are some old references to autoprocess name
  /// </summary>
  [Migration(165)]
  public class FixAutoSequenceTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixAutoSequenceTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS autoprocess_machinemoduleid_idx;");
      
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence DROP CONSTRAINT IF EXISTS fk_autoprocess_process;");
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence
  ADD CONSTRAINT fk_autosequence_sequence FOREIGN KEY (sequenceid)
      REFERENCES sequence (sequenceid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE;");
      
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence DROP CONSTRAINT IF EXISTS fk_autoprocess_machinemodule;");
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence
  ADD CONSTRAINT fk_autosequence_machinemodule FOREIGN KEY (machinemoduleid)
      REFERENCES machinemodule (machinemoduleid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE;");
      
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence DROP CONSTRAINT IF EXISTS autoprocess_pkey;");
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence
  ADD CONSTRAINT autosequence_pkey PRIMARY KEY(autosequenceid );");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence DROP CONSTRAINT autosequence_pkey;");
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence
  ADD CONSTRAINT autoprocess_pkey PRIMARY KEY(autosequenceid );");
      
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence DROP CONSTRAINT fk_autosequence_machinemodule;");
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence
  ADD CONSTRAINT fk_autoprocess_machinemodule FOREIGN KEY (machinemoduleid)
      REFERENCES machinemodule (machinemoduleid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE;");
      
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence DROP CONSTRAINT fk_autosequence_sequence;");
      Database.ExecuteNonQuery (@"ALTER TABLE autosequence
  ADD CONSTRAINT fk_autoprocess_process FOREIGN KEY (sequenceid)
      REFERENCES sequence (sequenceid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE;");
    }
  }
}
