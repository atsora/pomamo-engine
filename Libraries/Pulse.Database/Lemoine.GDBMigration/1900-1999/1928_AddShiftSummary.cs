// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1928: add the tables of the shift summaries
  ///
  /// <item>shiftsummary: highlights of what happened on a machine during a shift, at most one
  /// per machine, day and shift. The shift is null when no shift is defined: the summary is
  /// then the one of the day</item>
  /// <item>shiftsummarysection: data of a summary from a contributor, computed separately,
  /// and flagged (shiftsummarysectionupdaterequired) when it must be computed again</item>
  /// <item>shiftsummarycontent: rendering of a summary by a generator (Raw, a LLM, ...), with
  /// the versions of the sections it was rendered from</item>
  ///
  /// The summaries are computed by the analysis service, a little before the end of the shift,
  /// and again after its end while their data is processed or changes.
  /// </summary>
  [Migration (1928)]
  public class AddShiftSummary : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddShiftSummary).FullName);

    static readonly string SHIFT_SUMMARY = TableName.SHIFT_SUMMARY;
    static readonly string SHIFT_SUMMARY_ID = SHIFT_SUMMARY + "id";
    static readonly string SHIFT_SUMMARY_DAY = SHIFT_SUMMARY + "day";
    static readonly string SHIFT_SUMMARY_RANGE = SHIFT_SUMMARY + "datetimerange";

    static readonly string SECTION = TableName.SHIFT_SUMMARY_SECTION;
    static readonly string SECTION_KEY = SECTION + "key";
    static readonly string SECTION_ITEMS = SECTION + "items";
    static readonly string SECTION_UPDATE_REQUIRED = SECTION + "updaterequired";

    static readonly string CONTENT = TableName.SHIFT_SUMMARY_CONTENT;
    static readonly string CONTENT_GENERATOR = CONTENT + "generator";
    static readonly string CONTENT_CONTENT = CONTENT + "content";
    static readonly string CONTENT_TEXT = CONTENT + "text";
    static readonly string CONTENT_SOURCE_VERSION = CONTENT + "sourceversion";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (SHIFT_SUMMARY)) {
        AddShiftSummaryTable ();
      }
      if (!Database.TableExists (SECTION)) {
        AddSectionTable ();
      }
      if (!Database.TableExists (CONTENT)) {
        AddContentTable ();
      }
    }

    void AddShiftSummaryTable ()
    {
      Database.AddTable (SHIFT_SUMMARY,
                         new Column (SHIFT_SUMMARY_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (SHIFT_SUMMARY + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (SHIFT_SUMMARY_DAY, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (SHIFT_SUMMARY_RANGE, DbType.Int32, ColumnProperty.NotNull),
                         new Column (SHIFT_SUMMARY + "datetime", DbType.DateTime, ColumnProperty.NotNull));
      MakeColumnTsRange (SHIFT_SUMMARY, SHIFT_SUMMARY_RANGE);
      Database.GenerateForeignKey (SHIFT_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (SHIFT_SUMMARY, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      // A null shift must be unique too: COALESCE because NULLS NOT DISTINCT requires PostgreSQL 15
      Database.ExecuteNonQuery ($"""
        CREATE UNIQUE INDEX {SHIFT_SUMMARY}_unique_idx
        ON {SHIFT_SUMMARY} ({ColumnName.MACHINE_ID}, {SHIFT_SUMMARY_DAY}, COALESCE({ColumnName.SHIFT_ID}, 0))
        """);
      AddGistIndex (SHIFT_SUMMARY, ColumnName.MACHINE_ID, SHIFT_SUMMARY_RANGE);
    }

    void AddSectionTable ()
    {
      Database.AddTable (SECTION,
                         new Column (SECTION + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (SECTION + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (SHIFT_SUMMARY_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (SECTION_KEY, DbType.String, ColumnProperty.NotNull),
                         new Column (SECTION_ITEMS, DbType.String, ColumnProperty.NotNull),
                         new Column (SECTION + "datauntil", DbType.DateTime, ColumnProperty.Null),
                         new Column (SECTION_UPDATE_REQUIRED, DbType.Boolean, ColumnProperty.NotNull, true),
                         new Column (SECTION + "datetime", DbType.DateTime, ColumnProperty.Null));
      MakeColumnText (SECTION, SECTION_KEY);
      MakeColumnJson (SECTION, SECTION_ITEMS);
      Database.GenerateForeignKey (SECTION, SHIFT_SUMMARY_ID,
                                   SHIFT_SUMMARY, SHIFT_SUMMARY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (SECTION, SHIFT_SUMMARY_ID, SECTION_KEY);
      // The sections to compute
      AddIndexCondition (SECTION, $"{SECTION_UPDATE_REQUIRED} = TRUE", SHIFT_SUMMARY_ID);
    }

    void AddContentTable ()
    {
      Database.AddTable (CONTENT,
                         new Column (CONTENT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (CONTENT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (SHIFT_SUMMARY_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (CONTENT_GENERATOR, DbType.String, ColumnProperty.NotNull),
                         new Column (CONTENT_CONTENT, DbType.String, ColumnProperty.NotNull),
                         new Column (CONTENT_TEXT, DbType.String, ColumnProperty.Null),
                         new Column (CONTENT_SOURCE_VERSION, DbType.String, ColumnProperty.NotNull),
                         new Column (CONTENT + "datetime", DbType.DateTime, ColumnProperty.NotNull));
      MakeColumnText (CONTENT, CONTENT_GENERATOR);
      MakeColumnJson (CONTENT, CONTENT_CONTENT);
      MakeColumnText (CONTENT, CONTENT_TEXT);
      MakeColumnText (CONTENT, CONTENT_SOURCE_VERSION);
      Database.GenerateForeignKey (CONTENT, SHIFT_SUMMARY_ID,
                                   SHIFT_SUMMARY, SHIFT_SUMMARY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (CONTENT, SHIFT_SUMMARY_ID, CONTENT_GENERATOR);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      foreach (var table in new[] { CONTENT, SECTION, SHIFT_SUMMARY }) {
        if (Database.TableExists (table)) {
          Database.RemoveTable (table);
        }
      }
    }
  }
}
