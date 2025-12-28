// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add ProductionState foreign key to machineobservationstate and reason tables
  /// </summary>
  [Migration(1912)]
  public class AddProductionStateToMachineObservationStateAndReason: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddProductionStateToMachineObservationStateAndReason).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      // Add column to machineobservationstate table
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column (ColumnName.PRODUCTION_STATE_ID, System.Data.DbType.Int32));
      
      // Add foreign key constraint for machineobservationstate
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE, ColumnName.PRODUCTION_STATE_ID,
                                   TableName.PRODUCTION_STATE, ColumnName.PRODUCTION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      
      // Add column to reason table
      Database.AddColumn (TableName.REASON,
                          new Column (ColumnName.PRODUCTION_STATE_ID, System.Data.DbType.Int32));
      
      // Add foreign key constraint for reason
      Database.GenerateForeignKey (TableName.REASON, ColumnName.PRODUCTION_STATE_ID,
                                   TableName.PRODUCTION_STATE, ColumnName.PRODUCTION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      // Remove foreign key constraint from reason
      Database.RemoveForeignKey (TableName.REASON, $"fk_{TableName.REASON}_{TableName.PRODUCTION_STATE}");
      
      // Remove column from reason table
      Database.RemoveColumn (TableName.REASON, ColumnName.PRODUCTION_STATE_ID);
      
      // Remove foreign key constraint from machineobservationstate
      Database.RemoveForeignKey (TableName.MACHINE_OBSERVATION_STATE, $"fk_{TableName.MACHINE_OBSERVATION_STATE}_{TableName.PRODUCTION_STATE}");
      
      // Remove column from machineobservationstate table
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE, ColumnName.PRODUCTION_STATE_ID);
    }
  }
}