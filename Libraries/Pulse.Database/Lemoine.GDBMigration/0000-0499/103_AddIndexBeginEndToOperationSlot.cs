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
  /// Migration 103: Add some new indexes to the operationslot table:
  /// <item>(machineid, beginday, endday)</item>
  /// <item>(machineid, begindatetime, enddatetime)</item>
  /// </summary>
  [Migration(103)]
  public class AddIndexBeginEndToOperationSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexBeginEndToOperationSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "beginday",
                TableName.OPERATION_SLOT + "endday");
      
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "begindatetime",
                TableName.OPERATION_SLOT + "enddatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "beginday",
                   TableName.OPERATION_SLOT + "endday");
      
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "begindatetime",
                   TableName.OPERATION_SLOT + "enddatetime");
    }
  }
}
