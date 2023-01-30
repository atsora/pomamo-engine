// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 902: some columns might be in a json format, they will be changed to jsonB
  /// </summary>
  [Migration (902)]
  public class AlterJsonToJsonB : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AlterJsonToJsonB).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (GetColumnType(TableName.CNC_ALARM, TableName.CNC_ALARM + "properties") == "json") {
        MakeColumnJson (TableName.CNC_ALARM, TableName.CNC_ALARM + "properties");
      }

      if (GetColumnType (TableName.TOOL_POSITION, TableName.TOOL_POSITION + "properties") == "json") {
        MakeColumnJson (TableName.TOOL_POSITION, TableName.TOOL_POSITION + "properties");
      }

      if (GetColumnType (TableName.CURRENT_CNC_ALARM, TableName.CURRENT_CNC_ALARM + "properties") == "json") {
        MakeColumnJson (TableName.CURRENT_CNC_ALARM, TableName.CURRENT_CNC_ALARM + "properties");
      }

      if (GetColumnType (TableName.CNC_ALARM_SEVERITY_PATTERN, TableName.CNC_ALARM_SEVERITY_PATTERN + "pattern") == "json") {
        MakeColumnJson (TableName.CNC_ALARM_SEVERITY_PATTERN, TableName.CNC_ALARM_SEVERITY_PATTERN + "pattern");
      }

      // Two views on the table "event": they must be dropped first
      if (GetColumnType (TableName.EVENT, ColumnName.EVENT_DATA) == "json") {
        Database.ExecuteNonQuery ("DROP VIEW eventall");
        Database.ExecuteNonQuery ("DROP VIEW eventonly");

        MakeColumnJson (TableName.EVENT, ColumnName.EVENT_DATA);

        // Create the views
        Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.eventall
 AS
 SELECT event.eventid AS eventallid,
    event.eventid,
    event.eventlevelid,
    event.eventdatetime,
    event.eventtype,
    event.eventdata,
        CASE
            WHEN ""substring""(event.tableoid::regclass::citext::text, '^.........'::text) = 'pgfkpart.'::text THEN ""substring""(event.tableoid::regclass::citext::text, '(pgfkpart.)#""_ *#""([_]p[0-9]+)'::text, '#'::text)::citext
            ELSE event.tableoid::regclass::citext
        END AS eventtable
   FROM event
UNION ALL
 SELECT eventlongperiod.eventid AS eventallid,
    eventlongperiod.eventid,
    eventlongperiod.eventlevelid,
    eventlongperiod.eventdatetime,
    'EventLongPeriod'::character varying AS eventtype,
    '[]'::jsonb AS eventdata,
    'eventlongperiod'::citext AS eventtable
   FROM eventlongperiod
UNION ALL
 SELECT eventcncvalue.eventid AS eventallid,
    eventcncvalue.eventid,
    eventcncvalue.eventlevelid,
    eventcncvalue.eventdatetime,
    'EventCncValue'::character varying AS eventtype,
    '[]'::jsonb AS eventdata,
    'eventcncvalue'::citext AS eventtable
   FROM eventcncvalue
UNION ALL
 SELECT eventtoollife.eventid AS eventallid,
    eventtoollife.eventid,
    eventtoollife.eventlevelid,
    eventtoollife.eventdatetime,
    'EventToolLife'::character varying AS eventtype,
    '[]'::jsonb AS eventdata,
    'eventtoollife'::citext AS eventtable
   FROM eventtoollife;

GRANT SELECT ON TABLE public.eventall TO reportv2;
");
        Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.eventonly
 AS
 SELECT event.eventid,
    event.eventlevelid,
    event.eventdatetime,
    event.eventtype,
    event.eventdata
   FROM ONLY event;

GRANT SELECT ON TABLE public.eventonly TO reportv2;
");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Nothing
    }
  }
}
