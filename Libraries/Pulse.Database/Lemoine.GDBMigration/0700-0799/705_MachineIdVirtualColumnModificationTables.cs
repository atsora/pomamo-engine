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
  /// Migration 705: Add machineid virtual columns to all the modification tables
  /// </summary>
  [Migration (705)]
  public class MachineIdVirtualColumnModificationTables : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineIdVirtualColumnModificationTables).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddVirtualColumn (TableName.MACHINE_MODIFICATION, ColumnName.MACHINE_ID, "integer", @"
SELECT $1.machinemodificationmachineid
");
      AddVirtualColumn (TableName.MACHINE_MODIFICATION_STATUS, ColumnName.MACHINE_ID, "integer", @"
SELECT $1.machinemodificationstatusmachineid
");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DropVirtualColumn (TableName.MACHINE_MODIFICATION_STATUS, ColumnName.MACHINE_ID);
      DropVirtualColumn (TableName.MACHINE_MODIFICATION, ColumnName.MACHINE_ID);
    }
  }
}
