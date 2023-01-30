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
  /// Migration 156: Add a timestamp column to the monitoredmachine and operation tables, to track any update date/time
  /// </summary>
  [Migration(156)]
  public class AddTimestampsToOperationMonitoredMachine: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddTimestampsToOperationMonitoredMachine).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddTimeStamp (TableName.MONITORED_MACHINE);
      AddTimeStamp (TableName.OPERATION);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveTimeStamp (TableName.OPERATION);
      RemoveTimeStamp (TableName.MONITORED_MACHINE);
    }
    
    void AddTimeStamp (string tableName)
    {
      Database.AddColumn (tableName,
                          new Column (tableName + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      Database.ExecuteNonQuery (string.Format (@"CREATE OR REPLACE FUNCTION {0}_timestamp_update () RETURNS TRIGGER AS $$
BEGIN
  NEW.{0}timestamp := CURRENT_TIMESTAMP AT TIME ZONE 'GMT';
  RETURN NEW;
END
$$ LANGUAGE plpgsql",
                                               tableName));
      Database.ExecuteNonQuery (string.Format (@"CREATE TRIGGER {0}_insert_update BEFORE INSERT OR UPDATE
ON {0}
FOR EACH ROW
EXECUTE PROCEDURE {0}_timestamp_update ();",
                                               tableName));
    }
    
    void RemoveTimeStamp (string tableName)
    {
      Database.RemoveColumn (tableName,
                             tableName + "timestamp");
      Database.ExecuteNonQuery (string.Format ("DROP FUNCTION IF EXISTS {0}_timestamp_update () CASCADE",
                                               tableName));
    }
  }
}
