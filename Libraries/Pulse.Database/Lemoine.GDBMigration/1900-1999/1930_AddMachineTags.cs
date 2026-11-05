// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1930: add the machinetags column to the machine table
  ///
  /// Like the doctags column of the doc table (migration 1929), the tags are stored
  /// in a text[] column, not null, empty by default, with a gin index to search efficiently
  /// the machines that are associated to a tag:
  /// <item>machinetags @> ARRAY['tag']: machines that carry the tag (or all the tags of the array)</item>
  /// <item>machinetags &amp;&amp; ARRAY['tag1', 'tag2']: machines that carry at least one of the tags</item>
  /// </summary>
  [Migration (1930)]
  public class AddMachineTags : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineTags).FullName);

    static readonly string MACHINE_TAGS = TableName.MACHINE + "tags";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (Database.ColumnExists (TableName.MACHINE, MACHINE_TAGS)) {
        if (log.IsInfoEnabled) {
          log.Info ($"Up: column {MACHINE_TAGS} already exists in {TableName.MACHINE} => do nothing");
        }
        return;
      }

      // Note: no '{}' literal for the empty array, the SQL is formatted by the migrator
      Database.ExecuteNonQuery ($"""
ALTER TABLE {TableName.MACHINE}
ADD COLUMN {MACHINE_TAGS} text[] NOT NULL DEFAULT ARRAY[]::text[]
""");
      AddGinIndex (TableName.MACHINE, MACHINE_TAGS);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.ColumnExists (TableName.MACHINE, MACHINE_TAGS)) {
        // The index is removed with the column
        Database.RemoveColumn (TableName.MACHINE, MACHINE_TAGS);
      }
    }
  }
}
