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
  /// Migration 518:
  /// * add a column "cncsubinfo" for the cncalarm
  /// * cncalarmpattern can take this parameter into account
  /// * citext columns
  /// * one column renamed (stopstatus)
  /// </summary>
  [Migration(518)]
  public class CncAlarmUpdate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncAlarmUpdate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Add column cncsubinfo in cncalarm
      Database.AddColumn(TableName.CNC_ALARM, new Column(TableName.CNC_ALARM + "cncsubinfo", DbType.String));
      
      // citext columns
      Database.ExecuteNonQuery("ALTER TABLE cncalarm " +
                               "ALTER COLUMN cncalarmcncinfo " +
                               "SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery("ALTER TABLE cncalarm " +
                               "ALTER COLUMN cncalarmcncsubinfo " +
                               "SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery("ALTER TABLE cncalarm " +
                               "ALTER COLUMN cncalarmtype " +
                               "SET DATA TYPE CITEXT;");
      
      Database.ExecuteNonQuery("ALTER TABLE cncalarmseverity " +
                               "ALTER COLUMN cncalarmseveritycncinfo " +
                               "SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery("ALTER TABLE cncalarmseverity " +
                               "ALTER COLUMN cncalarmseverityname " +
                               "SET DATA TYPE CITEXT;");
      
      Database.ExecuteNonQuery("ALTER TABLE cncalarmseveritypattern " +
                               "ALTER COLUMN cncalarmseveritypatterncncinfo " +
                               "SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery("ALTER TABLE cncalarmseveritypattern " +
                               "ALTER COLUMN cncalarmseveritypatternname " +
                               "SET DATA TYPE CITEXT;");
      
      // Update the severity function
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
AND (p.cncalarmseveritypatternpattern->'cncsubinfo' IS NULL OR a.cncalarmcncsubinfo ~ (p.cncalarmseveritypatternpattern->>'cncsubinfo'))
AND (p.cncalarmseveritypatternpattern->'type' IS NULL OR a.cncalarmtype ~ (p.cncalarmseveritypatternpattern->>'type'))
AND (p.cncalarmseveritypatternpattern->'number' IS NULL OR a.cncalarmnumber ~ (p.cncalarmseveritypatternpattern->>'number'))
AND (p.cncalarmseveritypatternpattern->'message' IS NULL OR a.cncalarmmessage ~ (p.cncalarmseveritypatternpattern->>'message'))
AND (p.cncalarmseveritypatternpattern->'properties' IS NULL OR (a.cncalarmproperties)::jsonb @> (p.cncalarmseveritypatternpattern->'properties')::jsonb)
$BODY$
  LANGUAGE sql IMMUTABLE
  COST 100;");
      
      // Column cncalarmseverityprogramstopped becomes cncalarmseveritystopstatus
      Database.RenameColumn(TableName.CNC_ALARM_SEVERITY, TableName.CNC_ALARM_SEVERITY + "programstopped", TableName.CNC_ALARM_SEVERITY + "stopstatus");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Column cncalarmseveritystopstatus becomes cncalarmseverityprogramstopped
      Database.RenameColumn(TableName.CNC_ALARM_SEVERITY, TableName.CNC_ALARM_SEVERITY + "stopstatus", TableName.CNC_ALARM_SEVERITY + "programstopped");
      
      // Downgrade the severity function
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
      
      // Remove the column cncsubinfo
      Database.RemoveColumn(TableName.CNC_ALARM, TableName.CNC_ALARM + "cncsubinfo");
    }
  }
}
