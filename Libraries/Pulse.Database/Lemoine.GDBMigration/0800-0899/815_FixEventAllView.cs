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
  /// Migration 815: 
  /// </summary>
  [Migration (815)]
  public class FixEventAllView : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixEventAllView).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
DROP VIEW IF EXISTS eventall;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW eventall AS
SELECT event.eventid AS eventallid, event.*,
  CASE WHEN substring(event.tableoid::regclass::citext from '^.........') = 'pgfkpart.'
  THEN substring(event.tableoid::regclass::citext from '(pgfkpart.)#""_*#""([_]p[0-9]+)' for '#')::citext
  ELSE event.tableoid::regclass::citext
  END AS eventtable
FROM event
UNION ALL
SELECT eventid AS eventallid, eventid, eventlevelid, eventdatetime, 'EventLongPeriod' AS eventtype, '[]' AS eventdata, 'eventlongperiod' AS eventtable FROM eventlongperiod
UNION ALL
SELECT eventid AS eventallid, eventid, eventlevelid, eventdatetime, 'EventCncValue' AS eventtype, '[]' AS eventdata, 'eventcncvalue' AS eventtable FROM eventcncvalue
UNION ALL
SELECT eventid AS eventallid, eventid, eventlevelid, eventdatetime, 'EventToolLife' AS eventtype, '[]' AS eventdata, 'eventtoollife' AS eventtable FROM eventtoollife
");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"
DROP VIEW IF EXISTS eventall;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW eventall AS
SELECT event.eventid AS eventallid, event.*,
  CASE WHEN substring(event.tableoid::regclass::citext from '^.........') = 'pgfkpart.'
  THEN substring(event.tableoid::regclass::citext from '(pgfkpart.)?#""_*#""([_]p[0-9]+)' for '#')::citext
  ELSE event.tableoid::regclass::citext
  END AS eventtable
FROM event
UNION ALL
SELECT eventid AS eventallid, eventid, eventlevelid, eventdatetime, 'EventLongPeriod' AS eventtype, '[]' AS eventdata, 'eventlongperiod' AS eventtable FROM eventlongperiod
UNION ALL
SELECT eventid AS eventallid, eventid, eventlevelid, eventdatetime, 'EventCncValue' AS eventtype, '[]' AS eventdata, 'eventcncvalue' AS eventtable FROM eventcncvalue
UNION ALL
SELECT eventid AS eventallid, eventid, eventlevelid, eventdatetime, 'EventToolLife' AS eventtype, '[]' AS eventdata, 'eventtoollife' AS eventtable FROM eventtoollife
");
    }
  }
}
