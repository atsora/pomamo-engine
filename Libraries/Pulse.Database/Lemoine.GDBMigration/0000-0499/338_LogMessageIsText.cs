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
  /// Migration 338:
  /// </summary>
  [Migration(338)]
  public class LogMessageIsText: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (LogMessageIsText).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS analysislog;");
      
      ConvertMessageToText (TableName.DETECTION_ANALYSIS_LOG);
      ConvertMessageToText (TableName.GLOBAL_MODIFICATION_LOG);
      ConvertMessageToText (TableName.MACHINE_MODIFICATION_LOG);
      ConvertMessageToText (TableName.MAINTENANCE_LOG);
      ConvertMessageToText (TableName.SYNCHRONIZATION_LOG);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      ConvertMessageToVarchar255 (TableName.DETECTION_ANALYSIS_LOG);
      ConvertMessageToVarchar255 (TableName.GLOBAL_MODIFICATION_LOG);
      ConvertMessageToVarchar255 (TableName.MACHINE_MODIFICATION_LOG);
      ConvertMessageToVarchar255 (TableName.MAINTENANCE_LOG);
      ConvertMessageToVarchar255 (TableName.SYNCHRONIZATION_LOG);
      
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.analysislog AS 
 SELECT globalmodificationlog.logid,
    globalmodificationlog.datetime,
    globalmodificationlog.level,
    globalmodificationlog.message,
    globalmodificationlog.module,
    globalmodificationlog.modificationid
   FROM globalmodificationlog
UNION
 SELECT machinemodificationlog.logid,
    machinemodificationlog.datetime,
    machinemodificationlog.level,
    machinemodificationlog.message,
    machinemodificationlog.module,
    machinemodificationlog.modificationid
   FROM machinemodificationlog
UNION
 SELECT oldanalysislog.logid,
    oldanalysislog.datetime,
    oldanalysislog.level,
    oldanalysislog.message::text,
    oldanalysislog.module,
    oldanalysislog.modificationid
   FROM oldanalysislog;
");
    }
    
    void ConvertMessageToText (string tableName)
    {
      MakeColumnText (tableName, "message");
    }
    
    void ConvertMessageToVarchar255 (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE CHARACTER VARYING(255)",
                                               tableName, "message"));      
    }
  }
}
