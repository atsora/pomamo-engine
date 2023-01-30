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
  /// Migration 102: Add some new indexes to the reasonslot table:
  /// <item>(machineid, beginday, endday)</item>
  /// <item>(machineid, begindatetime, enddatetime)</item>
  /// </summary>
  [Migration(102)]
  public class AddIndexBeginEndToReasonSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexBeginEndToReasonSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "beginday",
                TableName.REASON_SLOT + "endday");
      
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "begindatetime",
                TableName.REASON_SLOT + "enddatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "beginday",
                   TableName.REASON_SLOT + "endday");
      
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "begindatetime",
                   TableName.REASON_SLOT + "enddatetime");
    }
  }
}
