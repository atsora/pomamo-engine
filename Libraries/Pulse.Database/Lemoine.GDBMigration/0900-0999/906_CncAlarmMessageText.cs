// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 906: text type for cncalarmmessage 
  /// </summary>
  [Migration (906)]
  public class CncAlarmMessageText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmMessageText).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      this.MakeColumnText (TableName.CNC_ALARM, TableName.CNC_ALARM + "message");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
