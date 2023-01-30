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
  /// Migration 85: add tables for "scanning" of workorder/serial code events
  /// </summary>
  [Migration(85)]
  public class AddWorkOrderSerialMachineStampTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddWorkOrderSerialMachineStampTables).FullName);
    
    /// <summary>
    /// Add workordermachinestamp table
    /// </summary>
    public void AddWorkOrderMachineStamp()
    {
      Database.AddTable(TableName.WORK_ORDER_MACHINE_STAMP,
                        new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.NotNull));

      Database.GenerateForeignKey (TableName.WORK_ORDER_MACHINE_STAMP, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.GenerateForeignKey (TableName.WORK_ORDER_MACHINE_STAMP, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.GenerateForeignKey (TableName.WORK_ORDER_MACHINE_STAMP, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      SetModificationTable (TableName.WORK_ORDER_MACHINE_STAMP);
    }
    
    /// <summary>
    /// Add serialnumbermachinestamp table
    /// </summary>
    public void AddSerialNumberMachineStamp()
    {
      
      Database.AddTable(TableName.SERIAL_NUMBER_MACHINE_STAMP,
                        new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column (ColumnName.SERIAL_NUMBER, DbType.String, ColumnProperty.NotNull));

      Database.GenerateForeignKey (TableName.SERIAL_NUMBER_MACHINE_STAMP, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.GenerateForeignKey (TableName.SERIAL_NUMBER_MACHINE_STAMP, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      SetModificationTable (TableName.SERIAL_NUMBER_MACHINE_STAMP);

    }
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddWorkOrderMachineStamp();
      AddSerialNumberMachineStamp();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='" + TableName.SERIAL_NUMBER_MACHINE_STAMP + "'");

      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='" + TableName.WORK_ORDER_MACHINE_STAMP + "'");

      Database.RemoveTable (TableName.SERIAL_NUMBER_MACHINE_STAMP);
      Database.RemoveTable (TableName.WORK_ORDER_MACHINE_STAMP);
    }
  }
}
