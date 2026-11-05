// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1929: add the doctags column to the doc table
  ///
  /// The tags are stored in a text[] column, not null, empty by default.
  /// A gin index on the column makes the search of the documents
  /// that are associated to a tag efficient, with the array operators:
  /// <item>doctags @> ARRAY['tag']: documents that carry the tag (or all the tags of the array)</item>
  /// <item>doctags &amp;&amp; ARRAY['tag1', 'tag2']: documents that carry at least one of the tags</item>
  /// </summary>
  [Migration (1929)]
  public class AddDocTags : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddDocTags).FullName);

    static readonly string DOC_TAGS = TableName.DOC + "tags";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (Database.ColumnExists (TableName.DOC, DOC_TAGS)) {
        if (log.IsInfoEnabled) {
          log.Info ($"Up: column {DOC_TAGS} already exists in {TableName.DOC} => do nothing");
        }
        return;
      }

      // Note: no '{}' literal for the empty array, the SQL is formatted by the migrator
      Database.ExecuteNonQuery ($"""
ALTER TABLE {TableName.DOC}
ADD COLUMN {DOC_TAGS} text[] NOT NULL DEFAULT ARRAY[]::text[]
""");
      AddGinIndex (TableName.DOC, DOC_TAGS);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.ColumnExists (TableName.DOC, DOC_TAGS)) {
        // The index is removed with the column
        Database.RemoveColumn (TableName.DOC, DOC_TAGS);
      }
    }
  }
}
