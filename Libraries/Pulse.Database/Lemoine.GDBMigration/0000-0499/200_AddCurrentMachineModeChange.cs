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
  /// Migration 200: add a currentmachinemodechange column that records the machine mode change date/time
  /// in table currentmachinemode
  /// </summary>
  [Migration(200)]
  public class AddCurrentMachineModeChange: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCurrentMachineModeChange).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.CURRENT_MACHINE_MODE,
                          new Column (TableName.CURRENT_MACHINE_MODE + "change", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0}
SET {0}change={0}datetime",
                                               TableName.CURRENT_MACHINE_MODE));
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION currentmachinemode_change_update () RETURNS TRIGGER AS $$
BEGIN
  IF OLD.machinemodeid<>NEW.machinemodeid THEN
    NEW.currentmachinemodechange=NEW.currentmachinemodedatetime;
  END IF;
  RETURN NEW;
END
$$ LANGUAGE plpgsql");
      Database.ExecuteNonQuery (@"CREATE TRIGGER currentmachinemode_update BEFORE UPDATE
ON currentmachinemode
FOR EACH ROW
EXECUTE PROCEDURE currentmachinemode_change_update ();");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CURRENT_MACHINE_MODE,
                             TableName.CURRENT_MACHINE_MODE + "change");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS currentmachinemode_change_update () CASCADE");
    }
  }
}
