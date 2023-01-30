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
  /// Migration 056: Add a detectionanalysislog table
  /// to register all errors from machine activity /
  /// auto-sequence and operation cycle analysis
  /// 
  /// Add also a foreign key to the modification table
  /// in the table analysislog
  /// </summary>
  [Migration(56)]
  public class DetectionAnalysisLog: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DetectionAnalysisLog).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddDetectionAnalysisLog ();
      AddForeignKeyAnalysisLog ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveDetectionAnalysisLog ();
    }
    
    void AddDetectionAnalysisLog ()
    {
      if (!Database.TableExists (TableName.DETECTION_ANALYSIS_LOG)) {
        Database.AddTable (TableName.DETECTION_ANALYSIS_LOG,
                           new Column (ColumnName.LOG_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("Level", DbType.String, ColumnProperty.NotNull),
                           new Column ("Message", DbType.String, ColumnProperty.NotNull),
                           new Column ("Module", DbType.String),
                           new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32));
        Database.ExecuteNonQuery (@"
ALTER TABLE detectionanalysislog
ALTER COLUMN datetime
SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.ExecuteNonQuery (@"
ALTER TABLE detectionanalysislog
ALTER COLUMN logid
SET DEFAULT nextval('log_logid_seq'::regclass)");
        Database.ExecuteNonQuery (@"
CREATE INDEX detectionanalysislog_datetime_idx
ON detectionanalysislog (datetime);");
        Database.GenerateForeignKey (TableName.DETECTION_ANALYSIS_LOG, ColumnName.MACHINE_ID,
                                     TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (TableName.DETECTION_ANALYSIS_LOG, ColumnName.MACHINE_MODULE_ID,
                                     TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetNull);
      }
    }
    
    void RemoveDetectionAnalysisLog ()
    {
      if (Database.TableExists (TableName.DETECTION_ANALYSIS_LOG)) {
        Database.RemoveTable (TableName.DETECTION_ANALYSIS_LOG);
      }
    }
    
    void AddForeignKeyAnalysisLog ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM analysislog a
WHERE NOT EXISTS (SELECT 1 FROM modification m
WHERE a.modificationid=m.modificationid)");
      Database.GenerateForeignKey (TableName.ANALYSIS_LOG, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
  }
}
