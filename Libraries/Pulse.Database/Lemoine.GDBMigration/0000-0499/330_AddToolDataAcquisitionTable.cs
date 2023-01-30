// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.GDBMigration;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 330: remove the timestamp from the tables "toollife" and "toolposition"
  /// create the table "tooldataacquisition" and add the timestamp
  /// </summary>
  [Migration(330)]
  public class AddToolDataAcquisitionTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddToolDataAcquisitionTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Create the table "tooldataacquisition",
      // * add a foreign key to the machine modules
      // * unique constraint on the machine modules
      // * partition it
      Database.AddTable(TableName.TOOL_DATA_ACQUISITION,
                        new Column (ColumnName.TOOL_DATA_ACQUISITION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column (TableName.TOOL_DATA_ACQUISITION + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_DATA_ACQUISITION + "timestamp",
                                   DbType.DateTime, ColumnProperty.NotNull,
                                   "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      Database.GenerateForeignKey(TableName.TOOL_DATA_ACQUISITION, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint(TableName.TOOL_DATA_ACQUISITION, ColumnName.MACHINE_MODULE_ID);
      PartitionTable(TableName.TOOL_DATA_ACQUISITION, TableName.MACHINE_MODULE);
      
      // Remove the timestamp columns from the tables toollife and toolposition
      RemoveTimeStampTrigger(TableName.TOOL_LIFE);
      RemoveTimeStampTrigger(TableName.TOOL_POSITION);
      Database.RemoveColumn(TableName.TOOL_LIFE, TableName.TOOL_LIFE + "timestamp");
      Database.RemoveColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "timestamp");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove the table tooldataacquisition
      if (Database.TableExists (TableName.TOOL_DATA_ACQUISITION)) {
        if (IsPartitioned (TableName.TOOL_DATA_ACQUISITION)) {
          UnpartitionTable (TableName.TOOL_DATA_ACQUISITION);
        }
        Database.RemoveTable (TableName.TOOL_DATA_ACQUISITION);
      }
      
      // Add the timestamp columns to the tables toollife and toolposition
      Database.AddColumn(TableName.TOOL_LIFE,
                         new Column(TableName.TOOL_LIFE + "timestamp",
                                    DbType.DateTime, ColumnProperty.NotNull,
                                    "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      AddTimeStampTrigger(TableName.TOOL_LIFE);
      Database.AddColumn(TableName.TOOL_POSITION,
                         new Column(TableName.TOOL_POSITION + "timestamp",
                                    DbType.DateTime, ColumnProperty.NotNull,
                                    "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      AddTimeStampTrigger(TableName.TOOL_POSITION);
    }
  }
}
