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
  /// Migration 051: Add the view machineobservationstateboolean
  /// for Borland C++ code
  /// </summary>
  [Migration(51)]
  public class MachineObservationStateBoolean: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationStateBoolean).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW machineobservationstateboolean AS
 SELECT
    machineobservationstate.machineobservationstateid AS machineobservationstateid,
    machineobservationstate.machineobservationstatename AS machineobservationstatename,
    machineobservationstate.machineobservationstatetranslationkey AS machineobservationstatetranslationkey,
    CASE
            WHEN machineobservationstate.machineobservationstateuserrequired = false THEN 0
            WHEN machineobservationstate.machineobservationstateuserrequired = true  THEN 1
    END AS machineobservationstateuserrequired,
    CASE
            WHEN machineobservationstate.machineobservationstateonsite = false THEN 0
            WHEN machineobservationstate.machineobservationstateonsite = true  THEN 1
            ELSE 0
    END AS machineobservationstateonsite,
    machineobservationstate.machineobservationstateidsiteattendancechange AS machineobservationstateidsiteattendancechange,
    machineobservationstate.machineobservationstateversion AS machineobservationstateversion
FROM machineobservationstate;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS machineobservationstateboolean;");
    }
  }
}
