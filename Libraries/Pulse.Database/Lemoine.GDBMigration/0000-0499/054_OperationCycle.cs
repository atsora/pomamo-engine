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
  /// Migration 054: Adapt the database for the new operationcycle table
  /// </summary>
  [Migration(54)]
  public class OperationCycle: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycle).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {  
      AddOperationCycleTable ();
      CleanOtherTables ();
    }
    
    void AddOperationCycleTable ()
    {
      Database.AddTable (TableName.OPERATION_CYCLE,
                         new Column (ColumnName.OPERATION_CYCLE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("OperationCycleVersion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("OperationCycleBegin", DbType.DateTime),
                         new Column ("OperationCycleEnd", DbType.DateTime),
                         new Column (ColumnName.OPERATION_SLOT_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE, ColumnName.OPERATION_SLOT_ID,
                                   TableName.OPERATION_SLOT, ColumnName.OPERATION_SLOT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.OPERATION_CYCLE,
                ColumnName.MACHINE_ID,
                "OperationCycleBegin");
      AddIndex (TableName.OPERATION_CYCLE,
                ColumnName.MACHINE_ID,
                "OperationCycleEnd");
      Database.ExecuteNonQuery (@"CREATE INDEX
operationcycle_partialcycles
ON operationcycle
USING btree (machineid, operationcyclebegin)
WHERE operationcycleend IS NULL");
      AddIndex (TableName.OPERATION_CYCLE,
                ColumnName.OPERATION_SLOT_ID);
    }
    
    void CleanOtherTables ()
    {
      Database.RemoveColumn (TableName.AUTO_SEQUENCE, ColumnName.MODIFICATION_ID);

      Database.RemoveTable (TableName.OPERATION_CYCLE_PERIOD);
      
      // Modification tables are removed in migration 58
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
