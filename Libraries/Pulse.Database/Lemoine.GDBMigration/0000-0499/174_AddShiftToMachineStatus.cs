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
  /// Migration 174: Add a shift column to the machinestatus table
  /// </summary>
  [Migration(174)]
  public class AddShiftToMachineStatus: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftToMachineStatus).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn(TableName.MACHINE_STATUS,
                         new Column(ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.MACHINE_STATUS, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATUS,
                             ColumnName.SHIFT_ID);
    }
  }
}
