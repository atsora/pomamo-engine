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
  /// Migration 037: Remove the ActivityDetection table
  /// <item>Add the auto-process table</item>
  /// <item>Remove the old MachineModuleStatus table</item>
  /// <item>Create a new MachineModuleStatus table</item>
  /// <item>Remove the ActivityDetection table</item>
  /// </summary>
  [Migration(37)]
  public class RemoveActivityDetection: MigrationExt
  {
    static readonly string OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS = "autoprocessid";
    static readonly string OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS_DATETIME = "autoprocessdatetime";
    static readonly string OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS_BEGIN = "autoprocessbegin";
    static readonly string OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS_END = "autoprocessend";
    
    static readonly string ACTIVITY_ANALYSIS_DATETIME = "ActivityAnalysisDateTime";
    
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveActivityDetection).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.OLD_AUTO_SEQUENCE)) {
        AddAutoProcessTable ();
      }
      if (Database.TableExists (TableName.OLD_MACHINE_MODULE_STATUS)) {
        RemoveOldMachineModuleStatusTable ();
      }
      AddNewMachineModuleStatusTable ();
      if (Database.TableExists (TableName.ACTIVITY_DETECTION)) {
        RemoveActivityDetectionTable ();
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (!Database.TableExists (TableName.ACTIVITY_DETECTION)) {
        AddActivityDetectionTable ();
      }
      if (Database.TableExists (TableName.OLD_MACHINE_MODULE_STATUS)) {
        RemoveNewMachineModuleStatusTable ();
      }
      AddOldMachineModuleStatusTable ();
      if (Database.TableExists (TableName.OLD_AUTO_SEQUENCE)) {
        RemoveAutoProcessTable ();
      }
    }
    
    void AddAutoProcessTable ()
    {
      Database.AddTable (TableName.OLD_AUTO_SEQUENCE,
                         new Column (ColumnName.OLD_AUTO_SEQUENCE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("AutoProcessVersion", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.OLD_SEQUENCE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("AutoProcessBegin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("AutoProcessEnd", DbType.DateTime),
                         new Column ("AutoProcessActivityBegin", DbType.DateTime),
                         new Column ("AutoProcessActivityEnd", DbType.DateTime),
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32));
      AddIndex (TableName.OLD_AUTO_SEQUENCE,
                ColumnName.MACHINE_MODULE_ID);
      Database.GenerateForeignKey (TableName.OLD_AUTO_SEQUENCE, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OLD_AUTO_SEQUENCE, ColumnName.OLD_SEQUENCE_ID,
                                   TableName.OLD_SEQUENCE, ColumnName.OLD_SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OLD_AUTO_SEQUENCE, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.ExecuteNonQuery (@"INSERT INTO autoprocess
(machinemoduleid, processid, autoprocessbegin, autoprocessactivitybegin, autoprocessactivityend)
SELECT machinemoduleid, autoprocessid, autoprocessdatetime, autoprocessbegin, autoprocessend
FROM machinemodulestatus
WHERE autoprocessid IS NOT NULL");
    }
    
    void RemoveAutoProcessTable ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM machinemodulestatus");
      Database.ExecuteNonQuery (@"INSERT INTO machinemodulestatus
(machinemoduleid, autoprocessid, autoprocessdatetime, autoprocessbegin, autoprocessend)
SELECT machinemoduleid, processid, autoprocessbegin, autoprocessactivitybegin, autoprocessactivityend
FROM autoprocess
WHERE autoprocessend IS NOT NULL
AND autoprocessbegin<autoprocessend
ORDER BY autoprocessbegin DESC
LIMIT 1");
      Database.RemoveTable (TableName.OLD_AUTO_SEQUENCE);
    }
    
    void RemoveOldMachineModuleStatusTable ()
    {
      Database.RemoveTable (TableName.OLD_MACHINE_MODULE_STATUS);
    }
    
    void AddOldMachineModuleStatusTable ()
    {
      Database.AddTable (TableName.OLD_MACHINE_MODULE_STATUS,
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS, DbType.Int32),
                         new Column (OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS_DATETIME, DbType.DateTime),
                         new Column (OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS_BEGIN, DbType.DateTime),
                         new Column (OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS_END, DbType.DateTime));
      Database.GenerateForeignKey (TableName.OLD_MACHINE_MODULE_STATUS, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OLD_MACHINE_MODULE_STATUS, OLD_MACHINE_MODULE_STATUS_AUTO_PROCESS,
                                   TableName.OLD_SEQUENCE, ColumnName.OLD_SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.AddCheckConstraint ("machinemodulestatus_autoprocess_datetime",
                                   TableName.OLD_MACHINE_MODULE_STATUS,
                                   @"(autoprocessid IS NULL)
OR (autoprocessid IS NOT NULL
    AND autoprocessdatetime IS NOT NULL)");
    }
    
    void AddNewMachineModuleStatusTable ()
    {
      Database.AddTable (TableName.OLD_MACHINE_MODULE_STATUS,
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ACTIVITY_ANALYSIS_DATETIME, DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.OLD_MACHINE_MODULE_STATUS, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (@"insert into machinemodulestatus (machinemoduleid, activityanalysisdatetime)
select machinemoduleid, min (CASE WHEN analysisapplieddatetime IS NULL THEN modificationdatetime ELSE modificationdatetime END)
from activitydetection
natural join modification
left outer join modificationstatus using (modificationid)
where analysisstatusid is null or analysisstatusid in (0, 1)
group by machinemoduleid");
    }
    
    void RemoveNewMachineModuleStatusTable ()
    {
      Database.RemoveTable (TableName.OLD_MACHINE_MODULE_STATUS);
    }
    
    void RemoveActivityDetectionTable ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM activitydetection");
      Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='ActivityDetection'");
      Database.RemoveTable (TableName.ACTIVITY_DETECTION);
    }
    
    void AddActivityDetectionTable ()
    {
      Database.AddTable (TableName.ACTIVITY_DETECTION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.ACTIVITY_DETECTION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ACTIVITY_DETECTION, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ACTIVITY_DETECTION, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.ACTIVITY_DETECTION));
    }
  }
}
