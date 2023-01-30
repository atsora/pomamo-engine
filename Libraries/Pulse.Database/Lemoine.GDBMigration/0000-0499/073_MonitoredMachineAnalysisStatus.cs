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
  /// Migration 073: Rename the table machinemodulestatus to monitoredmachineanalysisstatus
  /// </summary>
  [Migration(73)]
  public class MonitoredMachineAnalysisStatus: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MonitoredMachineAnalysisStatus).FullName);
    
    static readonly string ACTIVITY_ANALYSIS_DATETIME = "activityanalysisdatetime";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddLinkOperationMonitoredMachineForeignKey ();
      RenameMachineModuleStatusToMonitoredMachineAnalysisStatus ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RenameMonitoredMachineAnalysisStatusToMachineModuleStatus ();
    }
    
    /// <summary>
    /// Upgrade the foreign key from the machine table to the monitoredmachine table
    /// </summary>
    void AddLinkOperationMonitoredMachineForeignKey ()
    {
      Database.RemoveForeignKey (TableName.LINK_OPERATION, "fk_linkoperation_machine");
      Database.GenerateForeignKey (TableName.LINK_OPERATION, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RenameMachineModuleStatusToMonitoredMachineAnalysisStatus ()
    {
      Database.AddTable (TableName.MONITORED_MACHINE_ANALYSIS_STATUS,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("monitoredmachineanalysisstatusversion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ACTIVITY_ANALYSIS_DATETIME, DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.MONITORED_MACHINE_ANALYSIS_STATUS, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (@"INSERT INTO monitoredmachineanalysisstatus (machineid, activityanalysisdatetime) 
SELECT machineid, activityanalysisdatetime
FROM machinemodulestatus NATURAL JOIN machinemodule");
      Database.RemoveTable (TableName.OLD_MACHINE_MODULE_STATUS);
    }
    
    void RenameMonitoredMachineAnalysisStatusToMachineModuleStatus ()
    {
      Database.AddTable (TableName.OLD_MACHINE_MODULE_STATUS,
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("machinemodulestatusversion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ACTIVITY_ANALYSIS_DATETIME, DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.OLD_MACHINE_MODULE_STATUS, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (@"INSERT INTO machinemodulestatus (machinemoduleid, activityanalysisdatetime) 
SELECT mainmachinemoduleid, activityanalysisdatetime
FROM monitoredmachineanalysisstatus NATURAL JOIN monitoredmachine");
      Database.RemoveTable (TableName.MONITORED_MACHINE_ANALYSIS_STATUS);
    }
  }
}
