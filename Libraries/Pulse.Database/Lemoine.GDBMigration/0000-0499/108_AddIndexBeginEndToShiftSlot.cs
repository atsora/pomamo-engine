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
  /// Migration 108: Add some new indexes to the shiftslot table:
  /// <item>(machineid, beginday, endday)</item>
  /// <item>(machineid, begindatetime, enddatetime)</item>
  /// </summary>
  [Migration(108)]
  public class AddIndexBeginEndToShiftSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexBeginEndToShiftSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "beginday",
                TableName.SHIFT_SLOT + "endday");
      
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "begindatetime",
                TableName.SHIFT_SLOT + "enddatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "beginday",
                   TableName.SHIFT_SLOT + "endday");
      
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "begindatetime",
                   TableName.SHIFT_SLOT + "enddatetime");
    }
  }
}
