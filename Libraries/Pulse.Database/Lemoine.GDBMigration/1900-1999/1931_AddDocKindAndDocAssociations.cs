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
  /// Migration 1931: kind of the documents, and association of the documents
  /// to the components and to the operation revisions
  ///
  /// <item>dockind: reference table of the kinds of document (NC main program, drawing, ...),
  /// with a name or a translation key, an optional icon, and the virtual column display</item>
  /// <item>doc.dockindid: optional kind of a document</item>
  /// <item>componentdoc: documents of a component</item>
  /// <item>operationrevisiondoc: documents of an operation revision. There is no operationrevision
  /// table yet: a revision is an operation in this version, so operationrevisionid references
  /// the operation table until the operationrevision table exists</item>
  ///
  /// A document is associated at most once to the same component or operation revision.
  /// </summary>
  [Migration (1931)]
  public class AddDocKindAndDocAssociations : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddDocKindAndDocAssociations).FullName);

    static readonly string DOC_KIND = TableName.DOC_KIND;
    static readonly string DOC_KIND_NAME = DOC_KIND + "name";
    static readonly string DOC_KIND_TRANSLATION_KEY = DOC_KIND + "translationkey";
    static readonly string DOC_KIND_ICON = DOC_KIND + "icon";

    /// <summary>
    /// Default kinds, by id
    /// </summary>
    static readonly string[] DEFAULT_TRANSLATION_KEYS = {
      "DocKindNcMainProgram",
      "DocKindNcSubProgram",
      "DocKindDrawing",
      "DocKindSetupInstructions",
      "DocKindWorkInstructions",
      "DocKindToolList",
      "DocKindInspectionPlan",
      "DocKindModel3D",
    };

    const string DISPLAY_LIST_BEFORE = """
'machinemode'::name,'machinestatetemplate'::name,'goaltype'::name,'eventlevel'::name,'role'::name,'machinemonitoringtype'::name,
    'machineobservationstate'::name,'operationtype'::name,'unit'::name,'workorderstatus'::name,'componenttype'::name,
    'productionstate'::name
""";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (!Database.TableExists (DOC_KIND)) {
        AddDocKindTable ();
      }
      if (!Database.ColumnExists (TableName.DOC, ColumnName.DOC_KIND_ID)) {
        AddDocKindColumn ();
      }
      if (!Database.TableExists (TableName.COMPONENT_DOC)) {
        AddAssociationTable (TableName.COMPONENT_DOC, ColumnName.COMPONENT_ID, TableName.COMPONENT, ColumnName.COMPONENT_ID);
      }
      if (!Database.TableExists (TableName.OPERATION_REVISION_DOC)) {
        AddAssociationTable (TableName.OPERATION_REVISION_DOC, ColumnName.OPERATION_REVISION_ID, TableName.OPERATION, ColumnName.OPERATION_ID);
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.TableExists (TableName.OPERATION_REVISION_DOC)) {
        Database.RemoveTable (TableName.OPERATION_REVISION_DOC);
      }
      if (Database.TableExists (TableName.COMPONENT_DOC)) {
        Database.RemoveTable (TableName.COMPONENT_DOC);
      }
      if (Database.ColumnExists (TableName.DOC, ColumnName.DOC_KIND_ID)) {
        Database.RemoveColumn (TableName.DOC, ColumnName.DOC_KIND_ID);
      }
      if (Database.TableExists (DOC_KIND)) {
        SetDisplayList (DISPLAY_LIST_BEFORE);
        Database.ExecuteNonQuery ($"DROP FUNCTION IF EXISTS public.display({DOC_KIND})");
        Database.ExecuteNonQuery ($"DROP FUNCTION IF EXISTS public.{DOC_KIND}display({DOC_KIND})");
        Database.RemoveTable (DOC_KIND);
      }
    }

    void AddDocKindTable ()
    {
      Database.AddTable (DOC_KIND,
                         new Column (ColumnName.DOC_KIND_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (DOC_KIND + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (DOC_KIND_NAME, DbType.String, ColumnProperty.Unique),
                         new Column (DOC_KIND_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                         new Column (DOC_KIND_ICON, DbType.String, ColumnProperty.Null));
      MakeColumnCaseInsensitive (DOC_KIND, DOC_KIND_NAME);
      MakeColumnText (DOC_KIND, DOC_KIND_TRANSLATION_KEY);
      MakeColumnText (DOC_KIND, DOC_KIND_ICON);
      AddConstraintNameTranslationKey (DOC_KIND, DOC_KIND_NAME, DOC_KIND_TRANSLATION_KEY);

      // Default kinds, their translations are inserted by TranslationDAO
      for (int i = 0; i < DEFAULT_TRANSLATION_KEYS.Length; ++i) {
        Database.ExecuteNonQuery ($"""
INSERT INTO {DOC_KIND} ({ColumnName.DOC_KIND_ID}, {DOC_KIND_TRANSLATION_KEY})
VALUES ({i + 1}, '{DEFAULT_TRANSLATION_KEYS[i]}')
""");
      }
      // Keep some ids for the next default kinds
      SetMinSequence (DOC_KIND, ColumnName.DOC_KIND_ID, 100);

      // Virtual column display, from the name or the translation key
      SetDisplayList (DISPLAY_LIST_BEFORE + $",'{DOC_KIND}'::name");
      Database.ExecuteNonQuery ($"SELECT displayfromnametranslationkeyupdate ('{DOC_KIND}')");
    }

    /// <summary>
    /// Set the list of the tables whose display is computed from a name and a translation key
    /// </summary>
    /// <param name="tables">list of the table names, separated by a comma</param>
    void SetDisplayList (string tables)
    {
      // Note: no brace in the SQL, it is formatted by the migrator
      Database.ExecuteNonQuery ($"""
CREATE OR REPLACE FUNCTION displayfromnametranslationkeylist() RETURNS SETOF NAME AS
$BODY$
  SELECT unnest(ARRAY[{tables}]) AS tablename;
$BODY$ LANGUAGE SQL IMMUTABLE;
""");
    }

    void AddDocKindColumn ()
    {
      Database.AddColumn (TableName.DOC, new Column (ColumnName.DOC_KIND_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.DOC, ColumnName.DOC_KIND_ID,
                                   DOC_KIND, ColumnName.DOC_KIND_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.DOC, ColumnName.DOC_KIND_ID);
    }

    /// <summary>
    /// Add an association table between an item and a doc, unique per item and doc
    /// </summary>
    /// <param name="table"></param>
    /// <param name="itemColumn"></param>
    /// <param name="itemTable">referenced table of the item</param>
    /// <param name="itemTableColumn">referenced column of the item table</param>
    void AddAssociationTable (string table, string itemColumn, string itemTable, string itemTableColumn)
    {
      Database.ExecuteNonQuery ($"""
CREATE TABLE {table} (
  {itemColumn} integer NOT NULL REFERENCES {itemTable} ({itemTableColumn}) ON DELETE CASCADE,
  {ColumnName.DOC_ID} integer NOT NULL REFERENCES {TableName.DOC} ({ColumnName.DOC_ID}) ON DELETE CASCADE,
  PRIMARY KEY ({itemColumn}, {ColumnName.DOC_ID})
)
""");
      // To find the items a doc is associated to
      AddIndex (table, ColumnName.DOC_ID);
    }
  }
}
