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
  /// Migration to add the isoperatingtime column to the machineobservationstate table
  ///
  /// It is nullable: null means it is not known whether the machine observation state
  /// corresponds to an operating time
  /// </summary>
  [Migration (1928)]
  public class AddMachineObservationStateIsOperatingTime : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineObservationStateIsOperatingTime).FullName);

    static readonly string IS_OPERATING_TIME = $"{TableName.MACHINE_OBSERVATION_STATE}isoperatingtime";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (Database.ColumnExists (TableName.MACHINE_OBSERVATION_STATE, IS_OPERATING_TIME)) {
        if (log.IsInfoEnabled) {
          log.Info ($"Up: column {IS_OPERATING_TIME} already exists in {TableName.MACHINE_OBSERVATION_STATE} => do nothing");
        }
        return;
      }

      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column (IS_OPERATING_TIME, DbType.Boolean, ColumnProperty.Null));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.ColumnExists (TableName.MACHINE_OBSERVATION_STATE, IS_OPERATING_TIME)) {
        Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE, IS_OPERATING_TIME);
      }
    }
  }
}
