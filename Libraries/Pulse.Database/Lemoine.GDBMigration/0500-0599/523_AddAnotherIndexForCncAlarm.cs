// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 523: index MachineModule > Datetimerange for CncAlarms
  /// </summary>
  [Migration(523)]
  public class AddAnotherIndexForCncAlarm: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof(AddAnotherIndexForCncAlarm).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      AddGistIndex(TableName.CNC_ALARM,
                   ColumnName.MACHINE_MODULE_ID,
                   TableName.CNC_ALARM + "datetimerange");
      Database.ExecuteNonQuery("ALTER TABLE cncalarm ALTER column cncalarmdatetimerange SET NOT NULL");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      RemoveIndex(TableName.CNC_ALARM,
                  ColumnName.MACHINE_MODULE_ID,
                  TableName.CNC_ALARM + "datetimerange");
    }
  }
}
