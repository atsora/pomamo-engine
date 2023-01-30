// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1210: create a foreign key between the sequencemilestone and the sequence tables
  /// </summary>
  [Migration (1210)]
  public class SequenceMilestoneForeignKey : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SequenceMilestoneForeignKey).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.GenerateForeignKey (TableName.SEQUENCE_MILESTONE, ColumnName.SEQUENCE_ID,
                                   TableName.SEQUENCE, ColumnName.SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveForeignKey (TableName.SEQUENCE_MILESTONE, ColumnName.SEQUENCE_ID);
    }
  }
}
