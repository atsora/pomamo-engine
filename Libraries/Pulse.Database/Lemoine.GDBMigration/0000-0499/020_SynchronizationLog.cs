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
  /// Migration 020: New log and synchronizationlog tables
  /// </summary>
  [Migration(20)]
  public class SynchronizationLog: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SynchronizationLog).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.SYNCHRONIZATION_LOG)) {
        Database.AddTable (TableName.SYNCHRONIZATION_LOG,
                           new Column (ColumnName.LOG_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("Level", DbType.String, ColumnProperty.NotNull),
                           new Column ("Message", DbType.String, ColumnProperty.NotNull),
                           new Column ("Module", DbType.String),
                           new Column ("XmlElement", DbType.String, 2047));
        Database.ExecuteNonQuery ("ALTER TABLE synchronizationlog " +
                                  "ALTER COLUMN datetime " +
                                  "SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.ExecuteNonQuery (@"CREATE SEQUENCE log_logid_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 1
  CACHE 1;");
        Database.ExecuteNonQuery (@"ALTER TABLE synchronizationlog
ALTER COLUMN logid
SET DEFAULT nextval('log_logid_seq'::regclass)");
        Database.ExecuteNonQuery (@"CREATE INDEX syncronizationlog_datetime_idx " +
                                  "ON synchronizationlog (datetime);");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.SYNCHRONIZATION_LOG)) {
        Database.RemoveTable (TableName.SYNCHRONIZATION_LOG);
        Database.ExecuteNonQuery (@"DROP SEQUENCE log_logid_seq");
      }
    }
  }
}
