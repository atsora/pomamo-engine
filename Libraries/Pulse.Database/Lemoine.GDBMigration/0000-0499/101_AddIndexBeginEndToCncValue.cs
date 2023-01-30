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
  /// Migration 101: Add an index (machine, begin, end) to the cncvalue table
  /// </summary>
  [Migration(101)]
  public class AddIndexBeginEndToCncValue: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexBeginEndToCncValue).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                TableName.CNC_VALUE + "begindatetime",
                TableName.CNC_VALUE + "enddatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.CNC_VALUE,
                   ColumnName.MACHINE_MODULE_ID,
                   TableName.CNC_VALUE + "begindatetime",
                   TableName.CNC_VALUE + "enddatetime");
    }
  }
}
