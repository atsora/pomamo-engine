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
  /// Migration 616: fix the view eventall when the table is not partitioned
  /// </summary>
  [Migration (616)]
  public class FixEventAllViewNotPartitioned : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixEventAllViewNotPartitioned).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
  {
      DropVirtualColumn (TableName.EVENT, TableName.EVENT + "table");
      AddVirtualColumn (TableName.EVENT, TableName.EVENT + "table", "citext",
                       @"
SELECT 
  CASE WHEN substring(event.tableoid::regclass::citext from '^.........') = 'pgfkpart.'
  THEN substring(event.tableoid::regclass::citext from '(pgfkpart.)?#""_*#""([_]p[0-9]+)' for '#')::citext
  ELSE event.tableoid::regclass::citext
  END
FROM event
WHERE $1.eventid=event.eventid
");

      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW eventall AS
SELECT event.*,
  CASE WHEN substring(event.tableoid::regclass::citext from '^.........') = 'pgfkpart.'
  THEN substring(event.tableoid::regclass::citext from '(pgfkpart.)?#""_*#""([_]p[0-9]+)' for '#')::citext
  ELSE event.tableoid::regclass::citext
  END AS eventtable
FROM event
UNION ALL
SELECT eventid, eventlevelid, eventdatetime, 'EventLongPeriod' AS eventtype, '[]' AS eventdata, 'eventlongperiod' AS eventtable FROM eventlongperiod
UNION ALL
SELECT eventid, eventlevelid, eventdatetime, 'EventCncValue' AS eventtype, '[]' AS eventdata, 'eventcncvalue' AS eventtable FROM eventcncvalue
UNION ALL
SELECT eventid, eventlevelid, eventdatetime, 'EventToolLife' AS eventtype, '[]' AS eventdata, 'eventtoollife' AS eventtable FROM eventtoollife
");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
  {
  }
}
}
