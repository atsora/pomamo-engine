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
  /// Migration 814: 
  /// </summary>
  [Migration (814)]
  public class ReviewIndexesModifications : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReviewIndexesModifications).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpModification (TableName.MACHINE_MODIFICATION);
      UpModification (TableName.GLOBAL_MODIFICATION);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DownModification (TableName.GLOBAL_MODIFICATION);
      DownModification (TableName.MACHINE_MODIFICATION);
    }

    void UpModification (string tableName)
    {
      RemoveIndex (tableName, ColumnName.MODIFICATION_ID, ColumnName.MODIFICATION_PRIORITY);
      AddIndexCondition (tableName,
                         "parentglobalmodificationid IS NULL AND parentmachinemodificationid IS NULL",
                         ColumnName.MODIFICATION_PRIORITY, ColumnName.MODIFICATION_ID);
      RemoveIndex (tableName, ColumnName.MODIFICATION_DATETIME, ColumnName.MODIFICATION_PRIORITY);
    }

    void DownModification (string tableName)
    {
      RemoveIndex (tableName, ColumnName.MODIFICATION_PRIORITY, ColumnName.MODIFICATION_ID);
      AddIndexCondition (tableName,
                         "parentglobalmodificationid IS NULL AND parentmachinemodificationid IS NULL",
                         ColumnName.MODIFICATION_ID,
                         ColumnName.MODIFICATION_PRIORITY);
      AddIndexCondition (tableName,
                         "parentglobalmodificationid IS NULL AND parentmachinemodificationid IS NULL",
                         ColumnName.MODIFICATION_DATETIME,
                         ColumnName.MODIFICATION_PRIORITY);
    }
  }
}
