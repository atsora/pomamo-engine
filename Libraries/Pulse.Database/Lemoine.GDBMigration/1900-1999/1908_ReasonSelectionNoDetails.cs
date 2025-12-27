// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1908: Reason selection no details parameter
  /// </summary>
  [Migration (1908)]
  public class ReasonSelectionNoDetails : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSelectionNoDetails).FullName);

    static readonly string REASON_SELECTION_NO_DETAILS_COLUMN = TableName.REASON_SELECTION + "nodetails";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.REASON_SELECTION,
        new Column (REASON_SELECTION_NO_DETAILS_COLUMN, DbType.Boolean, ColumnProperty.Null, "FALSE"));
      Database.ExecuteNonQuery ($"UPDATE {TableName.REASON_SELECTION} SET {REASON_SELECTION_NO_DETAILS_COLUMN}=FALSE;");
      SetNotNull (TableName.REASON_SELECTION, REASON_SELECTION_NO_DETAILS_COLUMN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_SELECTION, REASON_SELECTION_NO_DETAILS_COLUMN);
    }
  }
}
