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
  /// Migration 311:
  /// </summary>
  [Migration(311)]
  public class FixMachineInsertTrigger: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixMachineInsertTrigger).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION machine_observationstateslot_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO observationstateslot (machineid, observationstateslotdatetimerange, observationstateslotdayrange, machineobservationstateid)
    VALUES (NEW.machineid, '(,)'::tsrange, '(,)'::tsrange, 2);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION machine_observationstateslot_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO observationstateslot (machineid, observationstateslotbegindatetime, observationstateslotbeginday, machineobservationstateid)
    VALUES (NEW.machineid, '1970-01-01 00:00:00', '1970-01-01', 2);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
    }
  }
}
