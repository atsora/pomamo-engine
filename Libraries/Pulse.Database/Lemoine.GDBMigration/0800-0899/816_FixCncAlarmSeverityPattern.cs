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
  /// Migration 816: 
  /// </summary>
  [Migration (816)]
  public class FixCncAlarmSeverityPattern : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixCncAlarmSeverityPattern).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.cncalarmseverityid(currentcncalarm)
  RETURNS integer AS
$BODY$
SELECT p.cncalarmseverityid
FROM (
  SELECT *
  FROM currentcncalarm
  WHERE currentcncalarm.currentcncalarmid = $1.currentcncalarmid
) AS a
LEFT JOIN (
  SELECT cncalarmseveritypattern.*
  FROM cncalarmseveritypattern
  JOIN cncalarmseverity
  ON cncalarmseveritypattern.cncalarmseverityid = cncalarmseverity.cncalarmseverityid
  WHERE cncalarmseveritypattern.cncalarmseveritypatternstatus != 3
  AND cncalarmseverity.cncalarmseveritystatus != 3
) AS p
ON a.currentcncalarmcncinfo = p.cncalarmseveritypatterncncinfo
AND (p.cncalarmseveritypatternpattern->'cncsubinfo' IS NULL OR p.cncalarmseveritypatternpattern->>'cncsubinfo' = '' OR a.currentcncalarmcncsubinfo ~ (p.cncalarmseveritypatternpattern->>'cncsubinfo'))
AND (p.cncalarmseveritypatternpattern->'type' IS NULL OR p.cncalarmseveritypatternpattern->>'type' = '' OR a.currentcncalarmtype ~ (p.cncalarmseveritypatternpattern->>'type'))
AND (p.cncalarmseveritypatternpattern->'number' IS NULL OR p.cncalarmseveritypatternpattern->>'number' = '' OR a.currentcncalarmnumber ~ (p.cncalarmseveritypatternpattern->>'number'))
AND (p.cncalarmseveritypatternpattern->'message' IS NULL OR p.cncalarmseveritypatternpattern->>'message' = '' OR a.currentcncalarmmessage ~ (p.cncalarmseveritypatternpattern->>'message'))
AND (p.cncalarmseveritypatternpattern->'properties' IS NULL OR (a.currentcncalarmproperties)::jsonb @> (p.cncalarmseveritypatternpattern->'properties')::jsonb)
$BODY$
  LANGUAGE sql IMMUTABLE
  COST 100;");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
