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
  /// Migration 314:
  /// </summary>
  [Migration (314)]
  public class FixObservationStateSlotDayRange : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixObservationStateSlotDayRange).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      FixDayRange ();
      CreateView ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

    void FixDayRange ()
    {
      Database.ExecuteNonQuery (@"UPDATE observationstateslot 
SET observationstateslotdayrange=tsrange(lower(observationstateslotdayrange),upper(observationstateslotdayrange),'[]')
WHERE NOT isempty(observationstateslotdayrange)
  AND NOT upper_inf(observationstateslotdayrange)
");
      Database.ExecuteNonQuery (@"UPDATE observationstateslot 
SET observationstateslotdayrange=tsrange(begindayslot.day,enddayslot.day,'[]')
FROM dayslot begindayslot, dayslot enddayslot
WHERE isempty(observationstateslotdayrange)
  AND NOT lower_inf(observationstateslot.observationstateslotdatetimerange)
  AND NOT upper_inf(observationstateslot.observationstateslotdatetimerange)
  AND begindayslot.dayslotrange @> lower(observationstateslot.observationstateslotdatetimerange)
  AND enddayslot.dayslotrange @> upper(observationstateslot.observationstateslotdatetimerange)
");
    }

    void CreateView ()
    {
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

      // observationstateslotbor for Borland C++ is deprecated
    }
  }
}
