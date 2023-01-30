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
  /// Migration 168: add a sequencekind column to the sequence table.
  /// sequencekind is a string equal to "machining", "stop" or "optional stop".
  /// For the migration, the value is set to:
  /// * "machining" if the autoonly column is true,
  /// * "stop" if the autoonly column is false and the name is not "PULSE OPTIONAL STOP"
  /// * "optional stop" otherwise (autoonly false and name "PULSE OPTIONAL STOP")
  /// </summary>
  [Migration(168)]
  public class AddSequenceKind: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddSequenceKind).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.SEQUENCE,
                          new Column(TableName.SEQUENCE + "kind",
                                     DbType.String,
                                     ColumnProperty.NotNull,
                                     "'Machining'"));

      Database.ExecuteNonQuery(
        "UPDATE sequence " +
        "SET sequencekind = 'Stop' " +
        "WHERE sequenceautoonly = false AND sequencename <> 'PULSE OPTIONAL STOP';"
       );

      Database.ExecuteNonQuery(
        "UPDATE sequence " +
        "SET sequencekind = 'OptionalStop' " +
        "WHERE sequenceautoonly = false AND sequencename = 'PULSE OPTIONAL STOP';"
       );

    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("ALTER TABLE sequence " +
                                "DROP COLUMN sequencekind;");

    }
  }
}
