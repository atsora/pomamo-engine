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
  /// Migration 251: add the shiftslotbreak table
  /// </summary>
  [Migration(251)]
  public class AddShiftSlotBreak: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftSlotBreak).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.SHIFT_SLOT_BREAK,
                         new Column (TableName.SHIFT_SLOT_BREAK + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.SHIFT_SLOT_ID, DbType.Int32), // Nullable for NHibernate
                         new Column (TableName.SHIFT_SLOT_BREAK + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.SHIFT_SLOT_BREAK + "end", DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.SHIFT_SLOT_BREAK, ColumnName.SHIFT_SLOT_ID,
                                   TableName.SHIFT_SLOT, ColumnName.SHIFT_SLOT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.SHIFT_SLOT_BREAK, ColumnName.SHIFT_SLOT_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.SHIFT_SLOT_BREAK);
    }
  }
}
