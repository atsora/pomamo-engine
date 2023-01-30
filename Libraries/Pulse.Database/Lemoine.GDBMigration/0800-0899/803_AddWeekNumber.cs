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
  /// Migration 803: add the week number (and year) to the dayslot table
  /// </summary>
  [Migration (803)]
  public class AddWeekNumber : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddWeekNumber).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.DAY_SLOT,
        new Column ("weekyear", DbType.Int32));
      Database.AddColumn (TableName.DAY_SLOT,
        new Column ("weeknumber", DbType.Int32));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.DAY_SLOT, "weeknumber");
      Database.RemoveColumn (TableName.DAY_SLOT, "weekyear");
    }
  }
}
