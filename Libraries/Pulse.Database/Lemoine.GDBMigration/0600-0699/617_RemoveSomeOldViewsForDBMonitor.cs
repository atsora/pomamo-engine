// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 617: 
  /// </summary>
  [Migration (617)]
  public class RemoveSomeOldViewsForDBMonitor : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveSomeOldViewsForDBMonitor).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      DropView ("notexisting");
      DropView ("observationstateslotbor");
      DropView ("reasonbor");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RestoreReasonbor ();
      RestoreObservationstateslotbor ();
    }
    void RestoreReasonbor ()
    {
      Database.ExecuteNonQuery (@"
      CREATE OR REPLACE VIEW public.reasonbor AS
 SELECT reason.reasonid,
    reason.reasonname,
    reason.reasontranslationkey,
    reason.reasoncode AS code,
    reason.reasondescription AS description,
    reason.reasondescriptiontranslationkey AS descriptiontranslationkey,
    reasoncolor(reason.*) AS color,
    reason.reasonlinkoperationdirection,
    reason.reasongroupid,
    reason.reasonversion
   FROM reason;
");
    }

    void RestoreObservationstateslotbor ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.observationstateslotbor AS 
 SELECT observationstateslot.observationstateslotid,
    observationstateslot.observationstateslotversion,
    observationstateslot.machineid,
        CASE
            WHEN lower_inf(observationstateslot.observationstateslotdatetimerange) THEN '1970-01-01 00:00:00'::timestamp without time zone
            ELSE lower(observationstateslot.observationstateslotdatetimerange)
        END AS observationstateslotbegindatetime,
        CASE
            WHEN lower_inf(observationstateslot.observationstateslotdayrange) THEN '1970-01-01 00:00:00'::timestamp without time zone
            ELSE lower(observationstateslot.observationstateslotdayrange)::timestamp without time zone
        END AS observationstateslotbeginday,
        CASE
            WHEN upper_inf(observationstateslot.observationstateslotdatetimerange) THEN NULL::timestamp without time zone
            ELSE upper(observationstateslot.observationstateslotdatetimerange)
        END AS observationstateslotenddatetime,
        CASE
            WHEN upper_inf(observationstateslot.observationstateslotdayrange) THEN NULL::timestamp without time zone
            ELSE upper(observationstateslot.observationstateslotdayrange)::timestamp without time zone
        END AS observationstateslotendday,
    observationstateslot.machineobservationstateid,
    observationstateslot.userid,
    observationstateslot.shiftid
   FROM observationstateslot;
");
    }

    void DropView (string view)
    {
      string request = string.Format (@"
DROP VIEW IF EXISTS {0};
",
view);
      Database.ExecuteNonQuery (request);
    }
  }
}
