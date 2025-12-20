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
  /// Migration 1907: Operation slot scrap column
  /// </summary>
  [Migration (1907)]
  public class OperationSlotScrap : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationSlotScrap).FullName);

    readonly static string OPERATION_SLOT_SCRAP_COLUMN = "operationslotscrap";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
        new Column (OPERATION_SLOT_SCRAP_COLUMN, DbType.String));
      MakeColumnJson (TableName.OPERATION_SLOT, OPERATION_SLOT_SCRAP_COLUMN);

    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT, OPERATION_SLOT_SCRAP_COLUMN);
    }
  }
}
