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
  /// Migration 189: Add a reference to MachineStateTemplate in ObservationStateSlot
  /// </summary>
  [Migration(189)]
  public class AddMachineStateTemplateToSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineStateTemplateToSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.OBSERVATION_STATE_SLOT,
                                               ColumnName.MACHINE_OBSERVATION_STATE_ID));
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.Null));
      Database.AddCheckConstraint (TableName.OBSERVATION_STATE_SLOT + "stateconstraint",
                                   TableName.OBSERVATION_STATE_SLOT,
                                   string.Format ("{0} IS NOT NULL OR {1} IS NOT NULL",
                                                  ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                                  ColumnName.MACHINE_STATE_TEMPLATE_ID));
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT,
                             ColumnName.MACHINE_STATE_TEMPLATE_ID);
      // Note: the following direct command works while Database.ChangeColumn does not work
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} SET NOT NULL",
                                               TableName.OBSERVATION_STATE_SLOT,
                                               ColumnName.MACHINE_OBSERVATION_STATE_ID));
    }
  }
}
