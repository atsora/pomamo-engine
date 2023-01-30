// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Migrator.Framework;
using Lemoine.Core.Log;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 548: 
  /// </summary>
  [Migration (548)]
  public class RemoveCurrentMachineModeChangeUpdate : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveCurrentMachineModeChangeUpdate).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
DROP FUNCTION public.currentmachinemode_change_update() CASCADE;
");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
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
  }
}