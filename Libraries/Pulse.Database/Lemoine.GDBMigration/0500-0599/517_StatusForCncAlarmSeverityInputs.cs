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
  /// Migration 517:
  /// </summary>
  [Migration(517)]
  public class StatusForCncAlarmSeverityInputs: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (StatusForCncAlarmSeverityInputs).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Add columns
      Database.AddColumn(TableName.CNC_ALARM_SEVERITY,
                         new Column(TableName.CNC_ALARM_SEVERITY + "status",
                                    DbType.Int32, ColumnProperty.NotNull, 0));
      Database.AddColumn(TableName.CNC_ALARM_SEVERITY_PATTERN,
                         new Column(TableName.CNC_ALARM_SEVERITY_PATTERN + "name",
                                    DbType.String));
      Database.AddColumn(TableName.CNC_ALARM_SEVERITY_PATTERN,
                         new Column(TableName.CNC_ALARM_SEVERITY_PATTERN + "status",
                                    DbType.Int32, ColumnProperty.NotNull, 0));
      
      // Add a unique constraint
      AddUniqueConstraint(TableName.CNC_ALARM_SEVERITY_PATTERN,
                          TableName.CNC_ALARM_SEVERITY_PATTERN + "cncinfo",
                          TableName.CNC_ALARM_SEVERITY_PATTERN + "name");
      
      // Pattern and severities with status 3 are ignored
      Database.ExecuteNonQuery(@"
CREATE OR REPLACE FUNCTION public.cncalarmseverityid(cncalarm)
  RETURNS integer AS
$BODY$
SELECT p.cncalarmseverityid
FROM (
  SELECT *
  FROM cncalarm
  WHERE cncalarm.cncalarmid = $1.cncalarmid
) AS a
LEFT JOIN (
  SELECT cncalarmseveritypattern.*
  FROM cncalarmseveritypattern
  JOIN cncalarmseverity
  ON cncalarmseveritypattern.cncalarmseverityid = cncalarmseverity.cncalarmseverityid
  WHERE cncalarmseveritypattern.cncalarmseveritypatternstatus != 3
  AND cncalarmseverity.cncalarmseveritystatus != 3
) AS p
ON a.cncalarmcncinfo = p.cncalarmseveritypatterncncinfo
AND (p.cncalarmseveritypatternpattern->'type' IS NULL OR a.cncalarmtype ~ (p.cncalarmseveritypatternpattern->>'type'))
AND (p.cncalarmseveritypatternpattern->'number' IS NULL OR a.cncalarmnumber ~ (p.cncalarmseveritypatternpattern->>'number'))
AND (p.cncalarmseveritypatternpattern->'message' IS NULL OR a.cncalarmmessage ~ (p.cncalarmseveritypatternpattern->>'message'))
AND (p.cncalarmseveritypatternpattern->'properties' IS NULL OR (a.cncalarmproperties)::jsonb @> (p.cncalarmseveritypatternpattern->'properties')::jsonb)
$BODY$
  LANGUAGE sql IMMUTABLE
  COST 100;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Pattern and severities are not ignored
      Database.ExecuteNonQuery(@"
CREATE OR REPLACE FUNCTION public.cncalarmseverityid(cncalarm)
  RETURNS integer AS
$BODY$
SELECT p.cncalarmseverityid
FROM cncalarm AS a
LEFT JOIN cncalarmseveritypattern AS p
ON a.cncalarmcncinfo = p.cncalarmseveritypatterncncinfo
AND (p.cncalarmseveritypatternpattern->'type' IS NULL OR a.cncalarmtype ~ (p.cncalarmseveritypatternpattern->>'type'))
AND (p.cncalarmseveritypatternpattern->'number' IS NULL OR a.cncalarmnumber ~ (p.cncalarmseveritypatternpattern->>'number'))
AND (p.cncalarmseveritypatternpattern->'message' IS NULL OR a.cncalarmmessage ~ (p.cncalarmseveritypatternpattern->>'message'))
AND (p.cncalarmseveritypatternpattern->'properties' IS NULL OR (a.cncalarmproperties)::jsonb @> (p.cncalarmseveritypatternpattern->'properties')::jsonb)
WHERE a.cncalarmid = $1.cncalarmid
$BODY$
  LANGUAGE sql IMMUTABLE
  COST 100;");
      
      // Remove a unique constraint
      RemoveUniqueConstraint(TableName.CNC_ALARM_SEVERITY_PATTERN,
                             TableName.CNC_ALARM_SEVERITY_PATTERN + "cncinfo",
                             TableName.CNC_ALARM_SEVERITY_PATTERN + "name");
      
      // Remove columns
      Database.RemoveColumn(TableName.CNC_ALARM_SEVERITY, TableName.CNC_ALARM_SEVERITY + "status");
      Database.RemoveColumn(TableName.CNC_ALARM_SEVERITY_PATTERN, TableName.CNC_ALARM_SEVERITY_PATTERN + "name");
      Database.RemoveColumn(TableName.CNC_ALARM_SEVERITY_PATTERN, TableName.CNC_ALARM_SEVERITY_PATTERN + "status");
    }
  }
}
