// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add DefaultRate column to productionstate table
  /// </summary>
  [Migration(1914)]
  public class AddProductionStateDefaultRate : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddProductionStateDefaultRate).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      Database.AddColumn (TableName.PRODUCTION_STATE,
                          new Column ($"{TableName.PRODUCTION_STATE}defaultrate", System.Data.DbType.Double));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      Database.RemoveColumn (TableName.PRODUCTION_STATE, $"{TableName.PRODUCTION_STATE}defaultrate");
    }
  }
}