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
  /// Migration 201: fix the machine state template item table with the forgotten machineobservationstateid column
  /// </summary>
  [Migration(201)]
  public class FixMachineStateTemplateItem: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixMachineStateTemplateItem).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.ColumnExists (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                                  ColumnName.MACHINE_OBSERVATION_STATE_ID)) {
        Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                            new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull));
        Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ITEM, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                     TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
