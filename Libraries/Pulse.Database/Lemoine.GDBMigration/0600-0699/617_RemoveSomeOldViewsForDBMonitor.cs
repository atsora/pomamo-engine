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
      DropView ("sfkyreas");
      DropView ("sfkyrgrp");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RestoreSfkyrgrp ();
      RestoreSfkyreas ();
      RestoreReasonbor ();
      RestoreObservationstateslotbor ();
    }

    void RestoreSfkyrgrp ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.sfkyrgrp AS 
 SELECT reasongroup.reasongroupid - 20 AS rgrpid,
        CASE
            WHEN reasongroup.reasongroupname IS NULL THEN tname.translationvalue
            ELSE reasongroup.reasongroupname::character varying
        END AS grpname,
        CASE
            WHEN reasongroup.reasongroupdescription IS NULL AND tdesc.translationvalue IS NULL THEN ''::character varying
            WHEN reasongroup.reasongroupdescription IS NULL THEN tdesc.translationvalue
            ELSE reasongroup.reasongroupdescription
        END AS descr
   FROM reasongroup
     LEFT JOIN translation tname ON reasongroup.reasongrouptranslationkey::text = tname.translationkey::text AND tname.locale = ''::citext
     LEFT JOIN translation tdesc ON reasongroup.reasongroupdescriptiontranslationkey::text = tdesc.translationkey::text AND tdesc.locale = ''::citext;
");
    }

    void RestoreSfkyreas ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.sfkyreas AS 
 SELECT reason.reasonid - 20 AS rid,
        CASE
            WHEN reason.reasonname IS NULL THEN tname.translationvalue
            ELSE reason.reasonname::character varying
        END AS reason,
        CASE
            WHEN reason.reasoncode IS NULL THEN ''::character varying
            ELSE reason.reasoncode::character varying
        END AS code,
        CASE
            WHEN reason.reasondescription IS NULL AND tdesc.translationvalue IS NULL THEN ''::character varying
            WHEN reason.reasondescription IS NULL THEN tdesc.translationvalue
            ELSE reason.reasondescription
        END AS descr,
    reason.reasongroupid - 20 AS rgrpid,
    reason.reasonlinkoperationdirection AS linkwith
   FROM reason
     LEFT JOIN translation tname ON reason.reasontranslationkey::text = tname.translationkey::text AND tname.locale = ''::citext
     LEFT JOIN translation tdesc ON reason.reasondescriptiontranslationkey::text = tdesc.translationkey::text AND tdesc.locale = ''::citext
  WHERE reason.reasonid >= 15;
");
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
