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
  /// Migration 1925: add a new table doc
  /// 
  /// A doc references a document by its path. The document itself is not stored
  /// in the database: listing the versions, downloading and uploading a document
  /// is delegated to an implementation of IDocExtension, that is only keyed by the path.
  /// 
  /// The path is unique, so that a document may be unambiguously resolved from its path.
  /// Additional properties may be added to this table later.
  /// </summary>
  [Migration (1925)]
  public class AddDoc : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddDoc).FullName);

    static readonly string DOC_PATH = TableName.DOC + "path";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.DOC)) {
        log.Warn ($"Up: table {TableName.DOC} already exists, do nothing");
        return;
      }

      Database.AddTable (TableName.DOC,
                         new Column (ColumnName.DOC_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.DOC + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (DOC_PATH, DbType.String, ColumnProperty.NotNull));
      MakeColumnText (TableName.DOC, DOC_PATH);
      AddUniqueConstraint (TableName.DOC, DOC_PATH);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.DOC)) {
        Database.RemoveTable (TableName.DOC);
      }
    }
  }
}
