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
  /// Migration 255:
  /// </summary>
  [Migration(255)]
  public class AddMachineObservationStateTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineObservationStateTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                          new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                             ColumnName.MACHINE_STATE_TEMPLATE_ID);
    }
  }
}
