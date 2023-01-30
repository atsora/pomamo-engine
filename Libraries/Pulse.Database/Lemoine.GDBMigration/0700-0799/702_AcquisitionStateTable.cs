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
  /// Migration 702: change the table "acquisition status" into the table "acquisitionstate"
  ///                and separate each value into distinct rows
  /// </summary>
  [Migration(702)]
  public class AcquisitionStateTable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof(AddEventMachine).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Add the table "acquisitionstate"
      Database.AddTable(TableName.ACQUISITION_STATE,
                        new Column (ColumnName.ACQUISITION_STATE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.ACQUISITION_STATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.ACQUISITION_STATE + "key", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.ACQUISITION_STATE + "datetime", DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey(TableName.ACQUISITION_STATE, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint(TableName.ACQUISITION_STATE, ColumnName.MACHINE_MODULE_ID, TableName.ACQUISITION_STATE + "key");
      PartitionTable(TableName.ACQUISITION_STATE, TableName.MACHINE_MODULE);

      // Copy data from "acquisitionstatus" to "acquisitionstate"
      Database.ExecuteNonQuery (
        "INSERT INTO acquisitionstate (machinemoduleid, acquisitionstatekey, acquisitionstatedatetime) " +
        "SELECT machinemoduleid, 'Detection', acquisitionstatuslastdetection " +
        "FROM acquisitionstatus " +
        "WHERE acquisitionstatuslastdetection IS NOT NULL"
      );
      Database.ExecuteNonQuery (
        "INSERT INTO acquisitionstate (machinemoduleid, acquisitionstatekey, acquisitionstatedatetime) " +
        "SELECT machinemoduleid, 'Alarms', acquisitionstatuslastalarmacquisition " +
        "FROM acquisitionstatus " +
        "WHERE acquisitionstatuslastalarmacquisition IS NOT NULL"
      );
      Database.ExecuteNonQuery (
        "INSERT INTO acquisitionstate (machinemoduleid, acquisitionstatekey, acquisitionstatedatetime) " +
        "SELECT machinemoduleid, 'Tools', acquisitionstatuslasttoolacquisition " +
        "FROM acquisitionstatus " +
        "WHERE acquisitionstatuslasttoolacquisition IS NOT NULL"
      );

      // Delete table "acquisitionstatus"
      UnpartitionTable ("acquisitionstatus");
      Database.RemoveTable("acquisitionstatus");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Add the table "acquisitionstatus"
      Database.AddTable ("acquisitionstatus",
                        new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column ("acquisitionstatusversion", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column ("acquisitionstatuslasttoolacquisition", DbType.DateTime),
                        new Column ("acquisitionstatuslastalarmacquisition", DbType.DateTime),
                        new Column ("acquisitionstatuslastdetection", DbType.DateTime));
      Database.GenerateForeignKey ("acquisitionstatus", ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint ("acquisitionstatus", ColumnName.MACHINE_MODULE_ID);
      PartitionTable ("acquisitionstatus", TableName.MACHINE_MODULE);

      // Copy data from "acquisitionstate" to "acquisitionstatus"
      Database.ExecuteNonQuery (
        "INSERT INTO acquisitionstatus (machinemoduleid, acquisitionstatuslastdetection, acquisitionstatuslastalarmacquisition, acquisitionstatuslasttoolacquisition) " +
        "SELECT t0.machinemoduleid, t1.acquisitionstatedatetime AS \"detection\", t2.acquisitionstatedatetime AS \"alarms\", t3.acquisitionstatedatetime AS \"tools\" " +
        "FROM acquisitionstate AS t0 " +
        "LEFT JOIN acquisitionstate AS t1 ON t1.machinemoduleid = t0.machinemoduleid AND t1.acquisitionstatekey = 'detection' " +
        "LEFT JOIN acquisitionstate AS t2 ON t2.machinemoduleid = t0.machinemoduleid AND t2.acquisitionstatekey = 'alarms' " +
        "LEFT JOIN acquisitionstate AS t3 ON t3.machinemoduleid = t0.machinemoduleid AND t3.acquisitionstatekey = 'tools' " +
        "GROUP BY t0.machinemoduleid, t1.acquisitionstatedatetime, t2.acquisitionstatedatetime, t3.acquisitionstatedatetime"
      );

      // Delete table "acquisitionstate"
      UnpartitionTable (TableName.ACQUISITION_STATE);
      Database.RemoveTable(TableName.ACQUISITION_STATE);
    }
  }
}