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
  /// Migration 193: Add a machine filter to the reasonselection table
  /// </summary>
  [Migration(193)]
  public class ReasonSelectionMachineFilter: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSelectionMachineFilter).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.REASON_SELECTION,
                          new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.REASON_SELECTION, ColumnName.MACHINE_FILTER_ID,
                                   TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_SELECTION,
                             ColumnName.MACHINE_FILTER_ID);
    }
  }
}
