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
  /// Migration 542: add the new log table CncDataImportLog
  /// </summary>
  [Migration (542)]
  public class AddCncDataImportLog : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncDataImportLog).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddCncDataImportLogTable ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveCncDataImportLog ();
    }

    void AddCncDataImportLogTable ()
    {
      if (!Database.TableExists (TableName.CNC_DATA_IMPORT_LOG)) {
        Database.AddTable (TableName.CNC_DATA_IMPORT_LOG,
                           new Column (ColumnName.LOG_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("Level", DbType.String, ColumnProperty.NotNull),
                           new Column ("Message", DbType.String, ColumnProperty.NotNull),
                           new Column ("Module", DbType.String),
                           new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32));
        MakeColumnText (TableName.CNC_DATA_IMPORT_LOG, "message");
        Database.ExecuteNonQuery (@"
ALTER TABLE CncDataImportlog
ALTER COLUMN datetime
SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.ExecuteNonQuery (@"
ALTER TABLE CncDataImportlog
ALTER COLUMN logid
SET DEFAULT nextval('log_logid_seq'::regclass)");
        Database.GenerateForeignKey (TableName.CNC_DATA_IMPORT_LOG, ColumnName.MACHINE_ID,
                                     TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (TableName.CNC_DATA_IMPORT_LOG, ColumnName.MACHINE_MODULE_ID,
                                     TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetNull);
      }
    }

    void RemoveCncDataImportLog ()
    {
      if (Database.TableExists (TableName.CNC_DATA_IMPORT_LOG)) {
        Database.RemoveTable (TableName.CNC_DATA_IMPORT_LOG);
      }
    }
  }
}
