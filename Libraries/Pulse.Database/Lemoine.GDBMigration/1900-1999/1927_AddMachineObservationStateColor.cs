// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add the color column to the machineobservationstate table
  ///
  /// It is optional, like the color of the machinestatetemplate table: the web services
  /// generate a color on the fly when no color is set
  /// </summary>
  [Migration (1927)]
  public class AddMachineObservationStateColor : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineObservationStateColor).FullName);

    static readonly string COLOR = $"{TableName.MACHINE_OBSERVATION_STATE}color";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (Database.ColumnExists (TableName.MACHINE_OBSERVATION_STATE, COLOR)) {
        if (log.IsInfoEnabled) {
          log.Info ($"Up: column {COLOR} already exists in {TableName.MACHINE_OBSERVATION_STATE} => do nothing");
        }
        return;
      }

      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column (COLOR, DbType.String));
      AddConstraintColor (TableName.MACHINE_OBSERVATION_STATE, COLOR);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.ColumnExists (TableName.MACHINE_OBSERVATION_STATE, COLOR)) {
        Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE, COLOR);
      }
    }
  }
}
