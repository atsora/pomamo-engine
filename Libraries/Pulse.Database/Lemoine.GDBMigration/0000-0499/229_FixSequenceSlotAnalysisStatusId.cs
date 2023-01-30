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
  /// Migration 229: when the table sequence slot is partitioned, not all the references
  /// to the old column sequenceslotanalysisstatusid were removed
  /// </summary>
  [Migration(229)]
  public class FixSequenceSlotAnalysisStatusId: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixSequenceSlotAnalysisStatusId).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (IsPartitioned (TableName.SEQUENCE_SLOT)) {
        Database.ExecuteNonQuery (@"DELETE FROM pgfkpart.parentindex
WHERE table_name='sequenceslot'
  AND index_name = 'sequenceslot_sequenceslotanalysisstatusid_idx'");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
