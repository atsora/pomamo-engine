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
  /// Migration 202: Fix the UserMachineSlotMachine to use the machinestatetemplate
  /// </summary>
  [Migration(202)]
  public class FixUserMachineSlotMachine: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixUserMachineSlotMachine).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.ColumnExists (TableName.USER_MACHINE_SLOT_MACHINE,
                                  ColumnName.MACHINE_STATE_TEMPLATE_ID)) {
        Database.RemoveColumn (TableName.USER_MACHINE_SLOT_MACHINE,
                               ColumnName.MACHINE_OBSERVATION_STATE_ID);
        Database.AddColumn (TableName.USER_MACHINE_SLOT_MACHINE,
                            new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull));
        Database.GenerateForeignKey (TableName.USER_MACHINE_SLOT_MACHINE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                     TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.ColumnExists (TableName.USER_MACHINE_SLOT_MACHINE,
                                  ColumnName.MACHINE_STATE_TEMPLATE_ID)) {
        Database.RemoveColumn (TableName.USER_MACHINE_SLOT_MACHINE,
                               ColumnName.MACHINE_STATE_TEMPLATE_ID);
        Database.AddColumn (TableName.USER_MACHINE_SLOT_MACHINE,
                            new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull));
        Database.GenerateForeignKey (TableName.USER_MACHINE_SLOT_MACHINE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                     TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
    }
  }
}
