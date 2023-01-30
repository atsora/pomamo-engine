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
  /// Migration 041: add an Id and Version columns in the Summary tables:
  /// <item>MachineActivitySummary</item>
  /// <item>ReasonSummary</item>
  /// <item>MachineStatus</item>
  /// <item>MachineModuleStatus</item>
  /// </summary>
  [Migration(41)]
  public class AddIdVersionInSummaryTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIdVersionInSummaryTables).FullName);

    static readonly string MIG_SUFFIX = "Mig";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpdateMachineActivitySummary ();
      UpdateReasonSummary ();
      UpdateMachineStatus ();
      UpdateMachineModuleStatus ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void UpdateMachineActivitySummary ()
    {
      string day = "machineactivityday";
      
      RemoveIndex (TableName.MACHINE_ACTIVITY_SUMMARY,
                   ColumnName.MACHINE_ID,
                   day);
      RemoveUniqueConstraint (TableName.MACHINE_ACTIVITY_SUMMARY,
                              ColumnName.MACHINE_ID,
                              day,
                              ColumnName.MACHINE_OBSERVATION_STATE_ID,
                              ColumnName.MACHINE_MODE_ID);
      Database.RenameTable (TableName.MACHINE_ACTIVITY_SUMMARY,
                            TableName.MACHINE_ACTIVITY_SUMMARY + MIG_SUFFIX);
      RemoveSequence ("machineactivitysummary_machineactivitysummaryid_seq");
      
      Database.AddTable (TableName.MACHINE_ACTIVITY_SUMMARY,
                         new Column (ColumnName.MACHINE_ACTIVITY_SUMMARY_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.MACHINE_ACTIVITY_SUMMARY_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (day, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("machineactivitytime", DbType.Double, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.MACHINE_ACTIVITY_SUMMARY,
                ColumnName.MACHINE_ID,
                day);
      AddUniqueConstraint (TableName.MACHINE_ACTIVITY_SUMMARY,
                           ColumnName.MACHINE_ID,
                           day,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.MACHINE_MODE_ID);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(machineid, machineactivityday, machineobservationstateid, machinemodeid, machineactivitytime)
SELECT machineid, machineactivityday, machineobservationstateid, machinemodeid, machineactivitytime
FROM {0}{1}",
                                               TableName.MACHINE_ACTIVITY_SUMMARY,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.MACHINE_ACTIVITY_SUMMARY + MIG_SUFFIX);
    }
    
    void UpdateReasonSummary ()
    {
      string day = "reasonsummaryday";
      
      RemoveIndex (TableName.REASON_SUMMARY,
                   ColumnName.MACHINE_ID,
                   day);
      RemoveUniqueConstraint (TableName.REASON_SUMMARY,
                              ColumnName.MACHINE_ID,
                              day,
                              ColumnName.MACHINE_OBSERVATION_STATE_ID,
                              ColumnName.REASON_ID);
      RemoveIndex (TableName.REASON_SUMMARY,
                   ColumnName.REASON_ID,
                   day);
      Database.RenameTable (TableName.REASON_SUMMARY,
                            TableName.REASON_SUMMARY + MIG_SUFFIX);
      RemoveSequence ("reasonsummary_reasonsummaryid_seq");
      
      Database.AddTable (TableName.REASON_SUMMARY,
                         new Column (ColumnName.REASON_SUMMARY_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.REASON_SUMMARY_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (day, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("ReasonSummaryTime"));
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.REASON_SUMMARY,
                ColumnName.MACHINE_ID,
                day);
      AddUniqueConstraint (TableName.REASON_SUMMARY,
                           ColumnName.MACHINE_ID,
                           day,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.REASON_ID);
      AddIndex (TableName.REASON_SUMMARY,
                ColumnName.REASON_ID,
                day);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(machineid, reasonsummaryday, machineobservationstateid, reasonid, reasonsummarytime)
SELECT machineid, reasonsummaryday, machineobservationstateid, reasonid, reasonsummarytime
FROM {0}{1}",
                                               TableName.REASON_SUMMARY,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.REASON_SUMMARY + MIG_SUFFIX);
    }
    
    void UpdateMachineStatus ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS,
                          new Column ("machinestatusversion", DbType.Int32, ColumnProperty.NotNull, 1));
    }
    
    void UpdateMachineModuleStatus ()
    {
      Database.AddColumn (TableName.OLD_MACHINE_MODULE_STATUS,
                          new Column ("machinemodulestatusversion", DbType.Int32, ColumnProperty.NotNull, 1));
    }
  }
}
