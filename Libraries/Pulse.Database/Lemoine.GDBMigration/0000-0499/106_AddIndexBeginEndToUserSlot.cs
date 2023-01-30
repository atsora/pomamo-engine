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
  /// Migration 106: Add some new indexes to the userslot table:
  /// <item>(userid, beginday, endday)</item>
  /// <item>(userid, begindatetime, enddatetime)</item>
  /// </summary>
  [Migration(106)]
  public class AddIndexBeginEndToUserSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexBeginEndToUserSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "beginday",
                TableName.USER_SLOT + "endday");
      
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "begindatetime",
                TableName.USER_SLOT + "enddatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "beginday",
                   TableName.USER_SLOT + "endday");
      
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "begindatetime",
                   TableName.USER_SLOT + "enddatetime");
    }
  }
}
