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
  /// Migration 243: add a maintenancelog table
  /// </summary>
  [Migration(243)]
  public class AddMaintenanceLog: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMaintenanceLog).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.MAINTENANCE_LOG)) {
        Database.AddTable (TableName.MAINTENANCE_LOG,
                           new Column (ColumnName.LOG_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("Level", DbType.String, ColumnProperty.NotNull),
                           new Column ("Message", DbType.String, ColumnProperty.NotNull),
                           new Column ("Module", DbType.String),
                           new Column (TableName.MAINTENANCE_LOG + "State", DbType.String, ColumnProperty.NotNull));
        Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN datetime
SET DEFAULT now() AT TIME ZONE 'UTC';",
                                                 TableName.MAINTENANCE_LOG));
        Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN logid
SET DEFAULT nextval('log_logid_seq'::regclass)",
                                                 TableName.MAINTENANCE_LOG));
        Database.ExecuteNonQuery (string.Format(@"
CREATE INDEX {0}_datetime_idx
ON {0} (datetime);",
                                                TableName.MAINTENANCE_LOG));
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.MAINTENANCE_LOG);
    }
  }
}
