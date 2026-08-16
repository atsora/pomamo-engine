// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add the productionclass column to the productionstate table
  /// </summary>
  [Migration (1921)]
  public class AddProductionStateProductionClass : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddProductionStateProductionClass).FullName);

    static readonly string PRODUCTION_CLASS = $"{TableName.PRODUCTION_STATE}productionclass";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (Database.ColumnExists (TableName.PRODUCTION_STATE, PRODUCTION_CLASS)) {
        if (log.IsInfoEnabled) {
          log.Info ($"Up: column {PRODUCTION_CLASS} already exists in {TableName.PRODUCTION_STATE} => do nothing");
        }
        return;
      }

      // Nullable for the moment: no production class is defined by default
      Database.AddColumn (TableName.PRODUCTION_STATE,
                          new Column (PRODUCTION_CLASS, System.Data.DbType.Int32, ColumnProperty.Null));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.ColumnExists (TableName.PRODUCTION_STATE, PRODUCTION_CLASS)) {
        Database.RemoveColumn (TableName.PRODUCTION_STATE, PRODUCTION_CLASS);
      }
    }
  }
}
