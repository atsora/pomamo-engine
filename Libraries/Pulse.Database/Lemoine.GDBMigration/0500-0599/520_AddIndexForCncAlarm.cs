// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 520: add an index for the table CncAlarm
  /// </summary>
  [Migration(520)]
  public class AddIndexForCncAlarm: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexForCncAlarm).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      AddIndex(TableName.CNC_ALARM, TableName.CNC_ALARM + "cncinfo", TableName.CNC_ALARM + "type");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      RemoveIndex(TableName.CNC_ALARM, TableName.CNC_ALARM + "cncinfo", TableName.CNC_ALARM + "type");
    }
  }
}
