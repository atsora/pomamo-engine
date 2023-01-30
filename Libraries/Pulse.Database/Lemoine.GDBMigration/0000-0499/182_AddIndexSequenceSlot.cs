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
  /// Migration 182: Add an index (machinemoduleid, sequenceslotend, sequenceslotbegin)
  /// </summary>
  [Migration(182)]
  public class AddIndexSequenceSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexSequenceSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      bool partitioned = IsPartitioned (TableName.SEQUENCE_SLOT);
      if (partitioned) {
        log.WarnFormat ("Up: " +
                        "sequenceslot is partitioned");
        Database.ExecuteNonQuery ("select pgfkpart.unpartition_with_fk ('public', 'sequenceslot');");
      }
      AddIndex (TableName.SEQUENCE_SLOT, ColumnName.MACHINE_MODULE_ID,
                TableName.SEQUENCE_SLOT + "end", TableName.SEQUENCE_SLOT + "begin");
      if (partitioned) {
        Database.ExecuteNonQuery ("select pgfkpart.partition_with_fk ('public', 'sequenceslot', 'public', 'machinemodule', FALSE);");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      bool partitioned = IsPartitioned (TableName.SEQUENCE_SLOT);
      if (partitioned) {
        log.WarnFormat ("Down: " +
                        "sequenceslot is partitioned");
        Database.ExecuteNonQuery ("select pgfkpart.unpartition_with_fk ('public', 'sequenceslot');");
      }
      RemoveIndex (TableName.SEQUENCE_SLOT, ColumnName.MACHINE_MODULE_ID,
                   TableName.SEQUENCE_SLOT + "end", TableName.SEQUENCE_SLOT + "begin");
      if (partitioned) {
        Database.ExecuteNonQuery ("select pgfkpart.partition_with_fk ('public', 'sequenceslot', 'public', 'machinemodule', FALSE);");
      }
    }
  }
}
