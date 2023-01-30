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
  /// Migration 701: add the table "acquisitionstatus"
  /// </summary>
  [Migration(701)]
  public class AddAcquisitionStatusTable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof(AddEventMachine).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Add the table "acquisitionstatus"
      Database.AddTable("acquisitionstatus",
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column("acquisitionstatusversion", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column("acquisitionstatuslasttoolacquisition", DbType.DateTime),
                        new Column("acquisitionstatuslastalarmacquisition", DbType.DateTime),
                        new Column ("acquisitionstatuslastdetection", DbType.DateTime));
      Database.GenerateForeignKey("acquisitionstatus", ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint("acquisitionstatus", ColumnName.MACHINE_MODULE_ID);
      PartitionTable("acquisitionstatus", TableName.MACHINE_MODULE);

      // Copy data from "tooldataacquisition" and "detectiontimestamp" to "acquisitionstatus"
      Database.ExecuteNonQuery (
        "INSERT INTO acquisitionstatus (machinemoduleid, acquisitionstatuslasttoolacquisition, acquisitionstatuslastdetection) " +
        "SELECT COALESCE(tooldataacquisition.machinemoduleid, detectiontimestamp.machinemoduleid), tooldataacquisitiontimestamp, detectiontimestamp " +
        "FROM tooldataacquisition FULL OUTER JOIN detectiontimestamp " +
        "ON tooldataacquisition.machinemoduleid = detectiontimestamp.machinemoduleid"
      );

      if (Database.TableExists (TableName.TOOL_DATA_ACQUISITION)) {
        Database.ExecuteNonQuery (string.Format ("ALTER TABLE {0} DROP CONSTRAINT IF EXISTS {1};",
           TableName.TOOL_DATA_ACQUISITION, "tooldataacquisition_machinemoduleid_unique"));
        // Delete table "tooldataacquisition"
        if (IsPartitioned (TableName.TOOL_DATA_ACQUISITION)) {
          UnpartitionTable (TableName.TOOL_DATA_ACQUISITION);
        }
        Database.RemoveTable (TableName.TOOL_DATA_ACQUISITION);
      }

      // Delete table "detectiontimestamp"
      if (IsPartitioned (TableName.DETECTION_TIMESTAMP)) {
        UnpartitionTable (TableName.DETECTION_TIMESTAMP);
      }
      Database.RemoveTable (TableName.DETECTION_TIMESTAMP);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Create table "tooldataacquisition"
      Database.AddTable(TableName.TOOL_DATA_ACQUISITION,
                        new Column(ColumnName.TOOL_DATA_ACQUISITION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.TOOL_DATA_ACQUISITION + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_DATA_ACQUISITION + "timestamp",
                                   DbType.DateTime, ColumnProperty.NotNull,
                                   "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      Database.GenerateForeignKey(TableName.TOOL_DATA_ACQUISITION, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint(TableName.TOOL_DATA_ACQUISITION, ColumnName.MACHINE_MODULE_ID);
      PartitionTable(TableName.TOOL_DATA_ACQUISITION, TableName.MACHINE_MODULE);

      // Create table "detectiontimestamp"
      Database.AddTable (TableName.DETECTION_TIMESTAMP,
                        new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column (TableName.DETECTION_TIMESTAMP + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column (TableName.DETECTION_TIMESTAMP, DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.DETECTION_TIMESTAMP, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);

      // Copy data from "acquisitionstatus" to "tooldataacquisition"
      Database.ExecuteNonQuery (
        "INSERT INTO tooldataacquisition (machinemoduleid, tooldataacquisitiontimestamp) " +
        "SELECT machinemoduleid, acquisitionstatuslasttoolacquisition " +
        "FROM acquisitionstatus " +
        "WHERE acquisitionstatuslasttoolacquisition IS NOT NULL"
      );

      // Copy data from "acquisitionstatus" to "detectiontimestamp"
      Database.ExecuteNonQuery (
        "INSERT INTO detectiontimestamp (machinemoduleid, detectiontimestamp) " +
        "SELECT machinemoduleid, acquisitionstatuslastdetection " +
        "FROM acquisitionstatus " +
        "WHERE acquisitionstatuslastdetection IS NOT NULL"
      );

      // Delete table "acquisitionstatus"
      UnpartitionTable ("acquisitionstatus");
      Database.RemoveTable("acquisitionstatus");
    }
  }
}