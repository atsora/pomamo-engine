// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add the capacitylevel column to the machineobservationstate table
  /// </summary>
  [Migration (1920)]
  public class AddMachineObservationStateCapacityLevel : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineObservationStateCapacityLevel).FullName);

    static readonly string CAPACITY_LEVEL = $"{TableName.MACHINE_OBSERVATION_STATE}capacitylevel";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (Database.ColumnExists (TableName.MACHINE_OBSERVATION_STATE, CAPACITY_LEVEL)) {
        if (log.IsInfoEnabled) {
          log.Info ($"Up: column {CAPACITY_LEVEL} already exists in {TableName.MACHINE_OBSERVATION_STATE} => do nothing");
        }
        return;
      }

      // Nullable for the moment: no capacity level is defined by default
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column (CAPACITY_LEVEL, System.Data.DbType.Int32, ColumnProperty.Null));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.ColumnExists (TableName.MACHINE_OBSERVATION_STATE, CAPACITY_LEVEL)) {
        Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE, CAPACITY_LEVEL);
      }
    }
  }
}
