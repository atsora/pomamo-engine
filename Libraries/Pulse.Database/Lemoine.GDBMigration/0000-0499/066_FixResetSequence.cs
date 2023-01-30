// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// /// <summary>
  /// Migration 066: Fix potential sequence issues for the following tables:
  /// <item>Reason</item>
  /// <item>ReasonGroup</item>
  /// <item>Tool</item>
  /// </summary>
  [Migration(66)]
  public class FixResetSequence: MigrationExt
  {
    
    static readonly ILog log = LogManager.GetLogger(typeof (FixResetSequence).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      ResetSequence (TableName.TOOL, ColumnName.TOOL_ID);
      ResetSequence (TableName.REASON,
                     ColumnName.REASON_ID);
      ResetSequence (TableName.REASON_GROUP,
                     ColumnName.REASON_GROUP_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
