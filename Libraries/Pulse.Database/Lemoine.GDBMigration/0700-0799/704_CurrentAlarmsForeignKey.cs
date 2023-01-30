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
  /// Migration 704: 
  /// </summary>
  [Migration (704)]
  public class CurrentAlarmsForeignKey : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CurrentAlarmsForeignKey).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Add a foreign key
      Database.GenerateForeignKey (TableName.CURRENT_CNC_ALARM, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);

      // Partition the table
      PartitionTable (TableName.CURRENT_CNC_ALARM, TableName.MACHINE_MODULE);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Unpartition
      UnpartitionTable (TableName.CURRENT_CNC_ALARM);

      // Remove a foreign key
      Database.RemoveForeignKey (TableName.CURRENT_CNC_ALARM, "fk_currentcncalarm_machinemodule");
    }
  }
}
