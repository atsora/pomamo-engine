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
  /// Migration 1006: add production state+rate columns to the reasonslot table 
  /// </summary>
  [Migration (1006)]
  public class ReasonSlotProductionState : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSlotProductionState).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.ColumnExists (TableName.REASON_SLOT, ColumnName.PRODUCTION_STATE_ID)) {
        Database.AddColumn (TableName.REASON_SLOT, new Column (ColumnName.PRODUCTION_STATE_ID, DbType.Int32));
        Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.PRODUCTION_STATE_ID, TableName.PRODUCTION_STATE, ColumnName.PRODUCTION_STATE_ID);
      }
      if (!Database.ColumnExists (TableName.REASON_SLOT, $"{TableName.REASON_SLOT}productionrate")) {
        Database.AddColumn (TableName.REASON_SLOT, new Column ($"{TableName.REASON_SLOT}productionrate", DbType.Double));
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_SLOT, $"{TableName.REASON_SLOT}productionrate");
      Database.RemoveColumn (TableName.REASON_SLOT, ColumnName.PRODUCTION_STATE_ID);
    }
  }
}
