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
  /// Migration 162: Remove the index reasonslot_machineid_reasonslotbegindatetime_idx
  /// because it is a duplicate of reasonslot_machineid_reasonslotbegindatetime_unique
  /// </summary>
  [Migration(162)]
  public class RemoveReasonSlotMachineIdeBeginDateTimeIndex: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveReasonSlotMachineIdeBeginDateTimeIndex).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "begindatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "begindatetime");
    }
  }
}
