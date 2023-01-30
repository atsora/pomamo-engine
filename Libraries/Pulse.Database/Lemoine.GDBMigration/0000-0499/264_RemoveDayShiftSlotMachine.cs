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
  /// Migration 264: remove two tables that won't be used
  /// </summary>
  [Migration(264)]
  public class RemoveDayShiftSlotMachine: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveDayShiftSlotMachine).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP TABLE IF EXISTS dayslotmachine;");
      Database.ExecuteNonQuery (@"DROP TABLE IF EXISTS shiftslotmachine;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
