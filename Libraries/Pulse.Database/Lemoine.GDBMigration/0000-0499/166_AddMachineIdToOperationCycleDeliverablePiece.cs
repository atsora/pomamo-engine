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
  /// Migration 166: Add a reference to the machine in operationcycledeliverablepiece table
  /// so that the operation cycle table may be partitioned
  /// </summary>
  [Migration(166)]
  public class AddMachineIdToOperationCycleDeliverablePiece: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineIdToOperationCycleDeliverablePiece).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                          new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.Null));
      Database.ExecuteNonQuery (@"UPDATE operationcycledeliverablepiece 
SET machineid=operationcycle.machineid
FROM operationcycle
WHERE operationcycle.operationcycleid=operationcycledeliverablepiece.operationcycleid");
      Database.ExecuteNonQuery (@"ALTER TABLE operationcycledeliverablepiece ALTER COLUMN machineid SET NOT NULL");
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                             ColumnName.MACHINE_ID);
    }
  }
}
