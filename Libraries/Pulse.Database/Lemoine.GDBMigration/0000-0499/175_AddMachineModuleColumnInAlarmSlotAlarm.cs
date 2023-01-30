// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 175: add the machinemoduleid in alarmslotalarm table
  /// </summary>
  [Migration(175)]
  public class AddMachineModuleColumnInAlarmSlotAlarm: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineModuleColumnInAlarmSlotAlarm).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn ("alarmslotalarm",
                          new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.Null));
      Database.ExecuteNonQuery (@"UPDATE alarmslotalarm
SET machinemoduleid=alarmslot.machinemoduleid
FROM alarmslot
WHERE alarmslotalarm.alarmslotid=alarmslot.alarmslotid");
      Database.ExecuteNonQuery (@"ALTER TABLE alarmslotalarm ALTER COLUMN machinemoduleid SET NOT NULL");
      Database.GenerateForeignKey ("alarmslotalarm", ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn ("alarmslotalarm",
                             ColumnName.MACHINE_MODULE_ID);
    }
  }
}
