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
  /// Migration 522: add the CurrentCncAlarm table
  /// </summary>
  [Migration (522)]
  public class AddCurrentCncAlarm : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCurrentCncAlarm).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.CURRENT_CNC_ALARM,
                        new Column (TableName.CURRENT_CNC_ALARM + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column (TableName.CURRENT_CNC_ALARM + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column (TableName.CURRENT_CNC_ALARM + "datetime", DbType.DateTime, ColumnProperty.NotNull),
                        new Column (TableName.CURRENT_CNC_ALARM + "cncinfo", DbType.String, ColumnProperty.NotNull),
                        new Column (TableName.CURRENT_CNC_ALARM + "cncsubinfo", DbType.String),
                        new Column (TableName.CURRENT_CNC_ALARM + "type", DbType.String, ColumnProperty.NotNull),
                        new Column (TableName.CURRENT_CNC_ALARM + "number", DbType.String, ColumnProperty.NotNull),
                        new Column (TableName.CURRENT_CNC_ALARM + "message", DbType.String),
                        new Column (TableName.CURRENT_CNC_ALARM + "properties", DbType.String)
                       );
      MakeColumnJson (TableName.CURRENT_CNC_ALARM, TableName.CURRENT_CNC_ALARM + "properties");
      MakeColumnText (TableName.CURRENT_CNC_ALARM, TableName.CURRENT_CNC_ALARM + "message");

      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.CURRENT_CNC_ALARM +
                               " ALTER COLUMN " + TableName.CURRENT_CNC_ALARM + "cncinfo" +
                               " SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.CURRENT_CNC_ALARM +
                               " ALTER COLUMN " + TableName.CURRENT_CNC_ALARM + "cncsubinfo" +
                               " SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.CURRENT_CNC_ALARM +
                               " ALTER COLUMN " + TableName.CURRENT_CNC_ALARM + "type" +
                               " SET DATA TYPE CITEXT;");

      // Add function to compute the severity on a CurrentCncAlarm
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
AND (p.cncalarmseveritypatternpattern->'cncsubinfo' IS NULL OR a.currentcncalarmcncsubinfo ~ (p.cncalarmseveritypatternpattern->>'cncsubinfo'))
AND (p.cncalarmseveritypatternpattern->'type' IS NULL OR a.currentcncalarmtype ~ (p.cncalarmseveritypatternpattern->>'type'))
AND (p.cncalarmseveritypatternpattern->'number' IS NULL OR a.currentcncalarmnumber ~ (p.cncalarmseveritypatternpattern->>'number'))
AND (p.cncalarmseveritypatternpattern->'message' IS NULL OR a.currentcncalarmmessage ~ (p.cncalarmseveritypatternpattern->>'message'))
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
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncalarmseverityid(currentcncalarm)");
      Database.RemoveTable (TableName.CURRENT_CNC_ALARM);
    }
  }
}
