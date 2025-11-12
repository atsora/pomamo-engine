// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1800:
  /// </summary>
  [Migration (1801)]
  public class FixCncAlarmMessage : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixCncAlarmMessage).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.CURRENT_CNC_ALARM, TableName.CURRENT_CNC_ALARM + "message");
      MakeColumnText (TableName.CNC_ALARM, TableName.CNC_ALARM + "message"); // Already done in 906
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
