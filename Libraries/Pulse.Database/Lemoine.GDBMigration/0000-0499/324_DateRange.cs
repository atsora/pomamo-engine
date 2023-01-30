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
  /// Migration 324: use daterange for dayranges
  /// </summary>
  [Migration(324)]
  public class DateRange: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DateRange).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS observationstatebeginendslot");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS observationstateslotbor");

      ConvertToDateRange (TableName.OBSERVATION_STATE_SLOT,
                          TableName.OBSERVATION_STATE_SLOT + "dayrange");

      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW observationstateslotbor AS
SELECT observationstateslotid, observationstateslotversion, machineid,
  CASE WHEN lower_inf(observationstateslotdatetimerange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdatetimerange)
  END AS observationstateslotbegindatetime,
  CASE WHEN lower_inf(observationstateslotdayrange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdayrange)::timestamp without time zone
  END AS observationstateslotbeginday,
  CASE WHEN upper_inf(observationstateslotdatetimerange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdatetimerange)
  END AS observationstateslotenddatetime,
  CASE WHEN upper_inf(observationstateslotdayrange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdayrange)::timestamp without time zone
  END AS observationstateslotendday,
  machineobservationstateid,
  userid, shiftid
FROM observationstateslot
");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW observationstatebeginendslot AS
SELECT observationstateslotid, observationstateslotversion, machineid, userid, shiftid,
  machineobservationstateid, machinestatetemplateid, observationstateslotproduction,
  CASE WHEN lower_inf(observationstateslotdatetimerange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdatetimerange)
  END AS observationstateslotbegindatetime,
  CASE WHEN upper_inf(observationstateslotdatetimerange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdatetimerange)::timestamp without time zone
  END AS observationstateslotenddatetime,
  CASE WHEN lower_inf(observationstateslotdayrange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdayrange)::timestamp without time zone
  END AS observationstateslotbeginday,
  CASE WHEN upper_inf(observationstateslotdayrange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdayrange)
  END AS observationstateslotendday
FROM observationstateslot
");
      
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.machine_observationstateslot_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO observationstateslot (machineid, observationstateslotdatetimerange, observationstateslotdayrange, machineobservationstateid)
    VALUES (NEW.machineid, '(,)'::tsrange, '(,)'::daterange, 2);
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
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS observationstatebeginendslot");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS observationstateslotbor");

      ConvertToTsrange (TableName.OBSERVATION_STATE_SLOT,
                        TableName.OBSERVATION_STATE_SLOT + "dayrange");

      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW observationstateslotbor AS
SELECT observationstateslotid, observationstateslotversion, machineid,
  CASE WHEN lower_inf(observationstateslotdatetimerange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdatetimerange)
  END AS observationstateslotbegindatetime,
  CASE WHEN lower_inf(observationstateslotdayrange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdayrange)
  END AS observationstateslotbeginday,
  CASE WHEN upper_inf(observationstateslotdatetimerange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdatetimerange)
  END AS observationstateslotenddatetime,
  CASE WHEN upper_inf(observationstateslotdayrange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdayrange)
  END AS observationstateslotendday,
  machineobservationstateid,
  userid, shiftid
FROM observationstateslot
");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW observationstatebeginendslot AS
SELECT observationstateslotid, observationstateslotversion, machineid, userid, shiftid,
  machineobservationstateid, machinestatetemplateid, observationstateslotproduction,
  CASE WHEN lower_inf(observationstateslotdatetimerange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdatetimerange)
  END AS observationstateslotbegindatetime,
  CASE WHEN upper_inf(observationstateslotdatetimerange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdatetimerange)
  END AS observationstateslotenddatetime,
  CASE WHEN lower_inf(observationstateslotdayrange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdayrange)
  END AS observationstateslotbeginday,
  CASE WHEN upper_inf(observationstateslotdayrange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdayrange)
  END AS observationstateslotendday
FROM observationstateslot
");

      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.machine_observationstateslot_inserter()
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
    
    void ConvertToDateRange (string tableName, string columnName)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} SET DATA TYPE daterange USING ({1}::varchar)::daterange;",
                                               tableName, columnName));
    }
    
    void ConvertToTsrange (string tableName, string columnName)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} SET DATA TYPE tsrange USING ({1}::varchar)::tsrange;",
                                               tableName, columnName));
    }
  }
}
