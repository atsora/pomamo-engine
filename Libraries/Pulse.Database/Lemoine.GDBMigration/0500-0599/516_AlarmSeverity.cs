// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 516:
  /// * table cncalarmseverity, listing all severities per CNC
  /// * table cncalarmseverity pattern, matching CNC alarms with their corresponding severity
  /// * virtual column cncalarm.severity, adding a severity to an alarm
  /// </summary>
  [Migration(516)]
  public class AlarmSeverity: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AlarmSeverity).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Table cncalarmseverity
      Database.AddTable(TableName.CNC_ALARM_SEVERITY,
                        new Column(ColumnName.CNC_ALARM_SEVERITY_ID, System.Data.DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.CNC_ALARM_SEVERITY + "version", System.Data.DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.CNC_ALARM_SEVERITY + "cncinfo", System.Data.DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM_SEVERITY + "name", System.Data.DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM_SEVERITY + "description", System.Data.DbType.String),
                        new Column(TableName.CNC_ALARM_SEVERITY + "programstopped", System.Data.DbType.String),
                        new Column(TableName.CNC_ALARM_SEVERITY + "color", System.Data.DbType.StringFixedLength, 7),
                        new Column(TableName.CNC_ALARM_SEVERITY + "focus", System.Data.DbType.Boolean)
                       );
      AddUniqueConstraint(TableName.CNC_ALARM_SEVERITY,
                          TableName.CNC_ALARM_SEVERITY + "cncinfo",
                          TableName.CNC_ALARM_SEVERITY + "name");
      
      // Table cncalarmseveritypattern
      Database.AddTable(TableName.CNC_ALARM_SEVERITY_PATTERN,
                        new Column(ColumnName.CNC_ALARM_SEVERITY_PATTERN_ID, System.Data.DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.CNC_ALARM_SEVERITY_PATTERN + "version", System.Data.DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.CNC_ALARM_SEVERITY_PATTERN + "cncinfo", System.Data.DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM_SEVERITY_PATTERN + "pattern", System.Data.DbType.String, ColumnProperty.NotNull),
                        new Column(ColumnName.CNC_ALARM_SEVERITY_ID, System.Data.DbType.Int32, ColumnProperty.NotNull)
                       );
      MakeColumnJson(TableName.CNC_ALARM_SEVERITY_PATTERN, TableName.CNC_ALARM_SEVERITY_PATTERN + "pattern");
      Database.GenerateForeignKey(TableName.CNC_ALARM_SEVERITY_PATTERN, ColumnName.CNC_ALARM_SEVERITY_ID,
                                  TableName.CNC_ALARM_SEVERITY, ColumnName.CNC_ALARM_SEVERITY_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // Virtual column cncalarmseverityid for the table cncalarm
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
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      Database.RemoveTable(TableName.CNC_ALARM_SEVERITY_PATTERN);
      Database.RemoveTable(TableName.CNC_ALARM_SEVERITY);
      Database.ExecuteNonQuery("DROP FUNCTION IF EXISTS public.cncalarmseverityid(cncalarm)");
    }
  }
}
