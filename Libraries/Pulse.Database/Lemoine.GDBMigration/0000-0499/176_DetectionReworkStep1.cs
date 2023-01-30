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
  /// Migration 176: Add a few tables and columns for the detection rework step 1
  /// </summary>
  [Migration(176)]
  public class DetectionReworkStep1: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DetectionReworkStep1).FullName);
    static readonly string MACHINE_MODULE_AUTO_SEQUENCE_ACTIVITY = TableName.MACHINE_MODULE + "AutoSequenceActivity";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpgradeMachineModule();
      AddNCProgramCodeTable();
      AddMachineModuleDetectionTable();
      AddDetectionTimeStampTable();
      AddMachineModuleAnalysisStatusTable();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveMachineModuleAnalysisStatusTable();
      RemoveDetectionTimeStampTable();
      RemoveMachineModuleDetectionTable();
      RemoveNCProgramCodeTable();
      DowngradeMachineModule();
    }
    
    void UpgradeMachineModule()
    {
      Database.AddColumn(TableName.MACHINE_MODULE,
                         new Column(MACHINE_MODULE_AUTO_SEQUENCE_ACTIVITY, DbType.String, ColumnProperty.NotNull, "'Machine'"));
    }
    
    void DowngradeMachineModule()
    {
      Database.RemoveColumn(TableName.MACHINE_MODULE,
                            MACHINE_MODULE_AUTO_SEQUENCE_ACTIVITY);
    }
    
    void AddNCProgramCodeTable ()
    {
      Database.AddTable (TableName.NC_PROGRAM_CODE,
                         new Column (TableName.NC_PROGRAM_CODE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.NC_PROGRAM_CODE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.NC_PROGRAM_CODE_KEY, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.NC_PROGRAM_CODE + "ResetMachineMode", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      AddUniqueIndex(TableName.NC_PROGRAM_CODE, ColumnName.NC_PROGRAM_CODE_KEY);
      Database.GenerateForeignKey (TableName.NC_PROGRAM_CODE, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.AddCheckConstraint("ncprogramcodemachinemode", TableName.NC_PROGRAM_CODE,
                                  string.Format ("NOT ({0} IS NOT NULL AND {1})",
                                                 ColumnName.MACHINE_MODE_ID, TableName.NC_PROGRAM_CODE + "ResetMachineMode"));
    }
    
    void RemoveNCProgramCodeTable ()
    {
      Database.RemoveTable(TableName.NC_PROGRAM_CODE);
    }
    
    void AddMachineModuleDetectionTable ()
    {
      Database.AddTable(TableName.MACHINE_MODULE_DETECTION,
                        new Column(TableName.MACHINE_MODULE_DETECTION + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.MACHINE_MODULE_DETECTION + "datetime", DbType.DateTime, ColumnProperty.NotNull),
                        new Column(TableName.MACHINE_MODULE_DETECTION + "name", DbType.String, ColumnProperty.Null),
                        new Column(ColumnName.STAMP_ID, DbType.Int32, ColumnProperty.Null),
                        new Column(ColumnName.NC_PROGRAM_CODE_KEY, DbType.String, ColumnProperty.Null),
                        new Column(TableName.MACHINE_MODULE_DETECTION + "StopNCProgram", DbType.Boolean, ColumnProperty.NotNull, "FALSE"),
                        new Column(TableName.MACHINE_MODULE_DETECTION + "StartCycle", DbType.Boolean, ColumnProperty.NotNull, "FALSE"),
                        new Column(TableName.MACHINE_MODULE_DETECTION + "StopCycle", DbType.Boolean, ColumnProperty.NotNull, "FALSE")
                       );
      AddIndex(TableName.MACHINE_MODULE_DETECTION, ColumnName.MACHINE_MODULE_ID, TableName.MACHINE_MODULE_DETECTION + "datetime");
      AddNamedIndex(TableName.MACHINE_MODULE_DETECTION + "_name",
                    TableName.MACHINE_MODULE_DETECTION, ColumnName.MACHINE_MODULE_ID, TableName.MACHINE_MODULE_DETECTION + "name",
                    TableName.MACHINE_MODULE_DETECTION + "datetime");
      Database.GenerateForeignKey(TableName.MACHINE_MODULE_DETECTION, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.MACHINE_MODULE_DETECTION, ColumnName.STAMP_ID,
                                  TableName.STAMP, ColumnName.STAMP_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey(TableName.MACHINE_MODULE_DETECTION, ColumnName.NC_PROGRAM_CODE_KEY,
                                  TableName.NC_PROGRAM_CODE, ColumnName.NC_PROGRAM_CODE_KEY,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void RemoveMachineModuleDetectionTable ()
    {
      Database.RemoveTable(TableName.MACHINE_MODULE_DETECTION);
    }
    
    void AddDetectionTimeStampTable ()
    {
      Database.AddTable(TableName.DETECTION_TIMESTAMP,
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(TableName.DETECTION_TIMESTAMP + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.DETECTION_TIMESTAMP, DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey(TableName.DETECTION_TIMESTAMP, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveDetectionTimeStampTable ()
    {
      Database.RemoveTable(TableName.DETECTION_TIMESTAMP);
    }
    
    void AddMachineModuleAnalysisStatusTable ()
    {
      Database.AddTable(TableName.MACHINE_MODULE_ANALYSIS_STATUS,
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(TableName.MACHINE_MODULE_ANALYSIS_STATUS + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column("LastMachineModuleDetectionId", DbType.Int32, ColumnProperty.NotNull, "0"),
                        new Column("AutoSequenceAnalysisDateTime", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')")
                       );
      Database.GenerateForeignKey(TableName.MACHINE_MODULE_ANALYSIS_STATUS, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveMachineModuleAnalysisStatusTable ()
    {
      Database.RemoveTable(TableName.MACHINE_MODULE_ANALYSIS_STATUS);
    }
  }
}
