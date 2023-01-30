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
  /// Migration 099: Because of sfktools, the minimum Id for the tool table must be 2
  /// </summary>
  [Migration(99)]
  public class ToolSequenceToMinimum2: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ToolSequenceToMinimum2).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Increment the sequence to be sure it starts at at least 2
      Database.ExecuteNonQuery (string.Format (@"
SELECT NEXTVAL('{0}_{1}_seq')",
                                               TableName.TOOL, ColumnName.TOOL_ID));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Do nothing
    }
  }
}
