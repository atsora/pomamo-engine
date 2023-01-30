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
  /// Migration 096: Add a shift column to the operation slot
  /// </summary>
  [Migration(96)]
  public class AddShiftToOperationSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftToOperationSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.OPERATION_SLOT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             ColumnName.SHIFT_ID);
    }
  }
}
