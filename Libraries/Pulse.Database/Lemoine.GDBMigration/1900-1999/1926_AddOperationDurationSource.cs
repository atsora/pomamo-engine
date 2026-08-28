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
  /// Migration to add the durationsource columns to the operation table
  /// 
  /// They store a <see cref="Lemoine.Model.DurationSource"/>: 0 for Manual, 1 for Auto, 2 for Extern
  /// </summary>
  [Migration (1926)]
  public class AddOperationDurationSource : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddOperationDurationSource).FullName);

    static readonly string MACHINING_DURATION_SOURCE = $"{TableName.OPERATION}machiningdurationsource";
    static readonly string LOADING_DURATION_SOURCE = $"{TableName.OPERATION}loadingdurationsource";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      AddDurationSourceColumn (MACHINING_DURATION_SOURCE);
      AddDurationSourceColumn (LOADING_DURATION_SOURCE);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      RemoveDurationSourceColumn (LOADING_DURATION_SOURCE);
      RemoveDurationSourceColumn (MACHINING_DURATION_SOURCE);
    }

    /// <summary>
    /// Add a duration source column to the operation table, if it does not exist yet
    /// </summary>
    /// <param name="columnName"></param>
    void AddDurationSourceColumn (string columnName)
    {
      if (Database.ColumnExists (TableName.OPERATION, columnName)) {
        if (log.IsInfoEnabled) {
          log.Info ($"AddDurationSourceColumn: column {columnName} already exists in {TableName.OPERATION} => do nothing");
        }
        return;
      }

      // 0 is DurationSource.Manual: the existing durations were all set manually
      Database.AddColumn (TableName.OPERATION,
                          new Column (columnName, DbType.Int32, ColumnProperty.NotNull, 0));
    }

    /// <summary>
    /// Remove a duration source column from the operation table, if it exists
    /// </summary>
    /// <param name="columnName"></param>
    void RemoveDurationSourceColumn (string columnName)
    {
      if (Database.ColumnExists (TableName.OPERATION, columnName)) {
        Database.RemoveColumn (TableName.OPERATION, columnName);
      }
    }
  }
}
