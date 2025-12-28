// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add IsSetup and LaborCost columns to machineobservationstate table
  /// </summary>
  [Migration(1910)]
  public class AddLaborCost: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddLaborCost).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column ("machineobservationstateissetup", System.Data.DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column ("laborcost", System.Data.DbType.Double, ColumnProperty.Null));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE, "machineobservationstatelaborcost");
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE, "machineobservationstateissetup");
    }
  }
}