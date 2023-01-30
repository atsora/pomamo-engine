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
  /// Migration 190: Use the Machine State Template in UserMachineAssociationMachine
  /// </summary>
  [Migration(190)]
  public class UserMachineStateTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UserMachineStateTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveColumn (TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                             ColumnName.MACHINE_OBSERVATION_STATE_ID);
      Database.AddColumn (TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                          new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.USER_MACHINE_ASSOCIATION_MACHINE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                             ColumnName.MACHINE_STATE_TEMPLATE_ID);
      Database.AddColumn (TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.USER_MACHINE_ASSOCIATION_MACHINE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
  }
}
