// Copyright (C) 2024 Atsora Solutions
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
  /// Migration 1703: Add a column reasondata in machinestatus table
  /// </summary>
  [Migration (1703)]
  public class AddReasonDataMachineStatus : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddReasonDataReasonSlot).FullName);

    readonly static string REASON_DATA_COLUMN = "reasondata";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS,
        new Column (REASON_DATA_COLUMN, DbType.String));
      MakeColumnJson (TableName.MACHINE_STATUS, REASON_DATA_COLUMN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATUS, REASON_DATA_COLUMN);
    }
  }
}
