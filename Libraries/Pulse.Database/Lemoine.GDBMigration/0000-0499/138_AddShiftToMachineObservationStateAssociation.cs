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
  /// Migration 138: add an optional Shift column to the MachineObservationStateAssociation table
  /// </summary>
  [Migration(138)]
  public class AddShiftToMachineObservationStateAssociation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftToMachineObservationStateAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                             ColumnName.SHIFT_ID);
    }
  }
}
