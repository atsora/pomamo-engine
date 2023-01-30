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
  /// Migration 180: update SerialNumberModification table (replace operationcycle ID by
  /// datetime + flag telling whether the datetime is a begin or an end.
  /// NB: this migration makes SerialNumberMachineStamp table obsolete
  /// </summary>
  [Migration(180)]
  public class UpdateSerialNumberModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UpdateSerialNumberModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Remove serialnumbermodification
      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='" + TableName.SERIAL_NUMBER_MODIFICATION + "'");

      Database.RemoveTable(TableName.SERIAL_NUMBER_MODIFICATION);
      
      // Add new version of serialnumbermodification
      Database.AddTable(TableName.SERIAL_NUMBER_MODIFICATION,
                        new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        // TODO: Warning ! Try to prefix the column names by the table name
                        // to ease the use of the natural joins
                        new Column("beginorenddatetime", DbType.DateTime, ColumnProperty.NotNull),
                        // TODO: Warning ! Try to prefix the column names by the table name
                        // to ease the use of the natural joins
                        new Column("isbegin", DbType.Boolean, ColumnProperty.NotNull),
                        new Column (ColumnName.SERIAL_NUMBER, DbType.String, ColumnProperty.NotNull));

      Database.GenerateForeignKey (TableName.SERIAL_NUMBER_MODIFICATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.GenerateForeignKey (TableName.SERIAL_NUMBER_MODIFICATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      SetModificationTable (TableName.SERIAL_NUMBER_MODIFICATION);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove serialnumbermodification
      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='" + TableName.SERIAL_NUMBER_MODIFICATION + "'");

      Database.RemoveTable(TableName.SERIAL_NUMBER_MODIFICATION);
      
      Database.AddTable(TableName.SERIAL_NUMBER_MODIFICATION,
                        new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(ColumnName.OPERATION_CYCLE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column (ColumnName.SERIAL_NUMBER, DbType.String, ColumnProperty.NotNull));

      Database.GenerateForeignKey (TableName.SERIAL_NUMBER_MODIFICATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.GenerateForeignKey (TableName.SERIAL_NUMBER_MODIFICATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.GenerateForeignKey (TableName.SERIAL_NUMBER_MODIFICATION, ColumnName.OPERATION_CYCLE_ID,
                                   TableName.OPERATION_CYCLE, ColumnName.OPERATION_CYCLE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      SetModificationTable (TableName.SERIAL_NUMBER_MODIFICATION);

    }
  }
}
