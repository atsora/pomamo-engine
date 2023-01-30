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
  /// Migration 549: 
  /// </summary>
  [Migration (549)]
  public class AddSequenceSlotIndexSequenceIdNotNull : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddSequenceSlotIndexSequenceIdNotNull).FullName);

    static readonly string INDEX_NAME = "sequenceslot_begin_sequenceid_not_null";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNamedIndexCondition (INDEX_NAME,
        TableName.SEQUENCE_SLOT, "sequenceid IS NOT NULL",
        ColumnName.MACHINE_MODULE_ID, TableName.SEQUENCE_SLOT + "begin");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex (INDEX_NAME, TableName.SEQUENCE_SLOT);
    }
  }
}