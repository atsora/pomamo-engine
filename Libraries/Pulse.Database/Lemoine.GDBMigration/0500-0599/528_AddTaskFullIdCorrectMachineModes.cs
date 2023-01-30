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
  /// Migration 528:
  /// <item>add a virtual column taskfullid, so that the id can be used in the display virtual column</item>
  /// <item>update the configuration of the machine modes</item>
  /// </summary>
  [Migration (528)]
  public class AddTaskFullIdCorrectMachineModes : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddTaskFullIdCorrectMachineModes).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddTaskFullId ();
      CorrectMachineModes ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

    void AddTaskFullId ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.taskfullid(taskfull)
RETURNS integer AS
$BODY$SELECT $1.taskid$BODY$
LANGUAGE sql IMMUTABLE
COST 100;");
    }

    void CorrectMachineModes ()
    {
      // Emergency => category machine error and parent = error
      Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecategoryid=3, parentmachinemodeid=10
WHERE machinemodeid=23 AND machinemodetranslationkey='MachineModeEmergency'
  AND machinemodeversion=2");
      // NoData => category unknown
      Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecategoryid=4
WHERE machinemodeid=8 AND machinemodetranslationkey='MachineModeNoData'
  AND machinemodeversion>1");
      // Unavailable => category unknown
      Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecategoryid=4
WHERE machinemodeid=9 AND machinemodetranslationkey='MachineModeUnavailable'
  AND machinemodeversion=2");
      // AcquisitionError => category unknown
      Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecategoryid=4
WHERE machinemodeid=59 AND machinemodetranslationkey='MachineModeAcquisitionError'
  AND machinemodeversion=1");
    }
  }
}
