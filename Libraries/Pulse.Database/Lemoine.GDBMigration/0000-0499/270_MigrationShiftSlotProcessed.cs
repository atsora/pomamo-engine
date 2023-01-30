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
  /// Migration 270:
  /// </summary>
  [Migration(270)]
  public class MigrationShiftSlotProcessed: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrationShiftSlotProcessed).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0}
SET {0}templateprocessed=FALSE, day=NULL
WHERE shifttemplateid IS NULL",
                                               TableName.SHIFT_SLOT));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
