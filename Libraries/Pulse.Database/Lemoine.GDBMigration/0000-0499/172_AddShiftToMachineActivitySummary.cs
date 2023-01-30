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
  /// Migration 172: Add a shift column to the machine activity summary table
  /// </summary>
  [Migration(172)]
  public class AddShiftToMachineActivitySummary: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftToMachineActivitySummary).FullName);
    
    static readonly string DAY_COLUMN = "machineactivityday";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveUniqueConstraint (TableName.MACHINE_ACTIVITY_SUMMARY,
                              ColumnName.MACHINE_ID,
                              DAY_COLUMN,
                              ColumnName.MACHINE_OBSERVATION_STATE_ID,
                              ColumnName.MACHINE_MODE_ID);
      Database.AddColumn (TableName.MACHINE_ACTIVITY_SUMMARY,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict); // Because of the unicity key
      AddUniqueConstraint (TableName.MACHINE_ACTIVITY_SUMMARY,
                           ColumnName.MACHINE_ID,
                           DAY_COLUMN,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.MACHINE_MODE_ID,
                           ColumnName.SHIFT_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUniqueConstraint (TableName.MACHINE_ACTIVITY_SUMMARY,
                              ColumnName.MACHINE_ID,
                              DAY_COLUMN,
                              ColumnName.MACHINE_OBSERVATION_STATE_ID,
                              ColumnName.MACHINE_MODE_ID,
                              ColumnName.SHIFT_ID);
      Database.RemoveColumn (TableName.MACHINE_ACTIVITY_SUMMARY,
                             ColumnName.SHIFT_ID);
      AddUniqueConstraint (TableName.MACHINE_ACTIVITY_SUMMARY,
                           ColumnName.MACHINE_ID,
                           DAY_COLUMN,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.MACHINE_MODE_ID);
    }
  }
}
