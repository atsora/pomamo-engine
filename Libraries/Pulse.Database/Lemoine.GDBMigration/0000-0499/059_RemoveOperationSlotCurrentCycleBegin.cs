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
  /// Migration 059: Remove the column currentcyclebegin of table operationslot
  /// </summary>
  [Migration(59)]
  public class RemoveOperationSlotCurrentCycleBegin: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveOperationSlotCurrentCycleBegin).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             "operationslotcurrentcyclebegin");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
