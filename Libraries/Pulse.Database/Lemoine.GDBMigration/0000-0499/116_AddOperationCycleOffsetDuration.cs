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
  /// Migration 116:
  /// - add "offsetduration" column in operation cycle
  /// (percentage representing the offset between operation cycle standard time,
  /// as defined by the operation estimatedmachinininghours colum, and the operation cycle
  /// actual time)
  /// - add "operationinformation" table which is a modification on the operation table
  /// (e.g. change of estimatedmachininghours)
  /// </summary>
  [Migration(116)]
  public class AddOperationCycleOffsetDuration: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationCycleOffsetDuration).FullName);
    static readonly string columnName1 = TableName.OPERATION_CYCLE + "offsetduration";
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION_CYCLE,
                          new Column (columnName1, DbType.Double));
      
      AddIndex(TableName.OPERATION_CYCLE,
               columnName1);
      
      Database.AddTable (TableName.OPERATION_INFORMATION,
                         new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("oldestimatedmachininghours", DbType.Double));

      Database.GenerateForeignKey (TableName.OPERATION_INFORMATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey (TableName.OPERATION_INFORMATION, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      AddIndex (TableName.OPERATION_INFORMATION,
                ColumnName.OPERATION_ID);

      SetModificationTable (TableName.OPERATION_INFORMATION);
      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_CYCLE, columnName1);
      
      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='" + TableName.OPERATION_INFORMATION + "'");

      Database.RemoveTable (TableName.OPERATION_INFORMATION);

    }
  }
}
