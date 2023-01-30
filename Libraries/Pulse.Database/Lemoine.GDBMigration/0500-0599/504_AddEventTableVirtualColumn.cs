// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 504:
  /// </summary>
  [Migration(504)]
  public class AddEventTableVirtualColumn: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEventTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddVirtualColumn (TableName.EVENT, TableName.EVENT + "table", "citext",
                       @"
SELECT substring(tableoid::regclass::citext from '(pgfkpart.)?#""_*#""([_]p[0-9]+)' for '#')::citext
FROM event
WHERE $1.eventid=event.eventid
");
      
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW eventall AS
SELECT event.*, substring(event.tableoid::regclass::citext from '(pgfkpart.)?#""_*#""([_]p[0-9]+)' for '#')::citext AS eventtable FROM event
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
      Database.ExecuteNonQuery (@"
DROP VIEW IF EXISTS eventall
");
      
      DropVirtualColumn (TableName.EVENT, TableName.EVENT + "table");
    }
  }
}
