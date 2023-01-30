// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 342:
  /// </summary>
  [Migration(342)]
  public class AddForeignKeyCncAlarm: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddForeignKeyCncAlarm).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Add a foreign key
      Database.GenerateForeignKey(TableName.CNC_ALARM, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove a foreign key
      Database.RemoveForeignKey(TableName.CNC_ALARM, "fk_cncalarm_machinemodule");
    }
  }
}
