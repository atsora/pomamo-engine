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
  /// Migration 137: Add a shift value to the ObservationStateSlot analysis table
  /// </summary>
  [Migration(137)]
  public class AddShiftToObservationStateSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftToObservationStateSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT,
                            ColumnName.SHIFT_ID);
    }
  }
}
