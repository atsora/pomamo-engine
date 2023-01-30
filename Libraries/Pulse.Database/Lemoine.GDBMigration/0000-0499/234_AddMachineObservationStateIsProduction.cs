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
  /// Migration 234:
  /// </summary>
  [Migration(234)]
  public class AddMachineObservationStateIsProduction: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineObservationStateIsProduction).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column (TableName.MACHINE_OBSERVATION_STATE + "isproduction", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      
      // Default IsProduction is TRUE for Attended (1) and Production (9)
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0}
SET {0}isproduction=TRUE
WHERE {0}id IN (1, 9)",
                                               TableName.MACHINE_OBSERVATION_STATE));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE,
                             TableName.MACHINE_OBSERVATION_STATE + "isproduction");
    }
  }
}
