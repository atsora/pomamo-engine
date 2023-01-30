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
  /// Migration 114: add a column for counting number of activity analysis
  /// </summary>
  [Migration(114)]
  public class AddActivityAnalysisProperties: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddActivityAnalysisProperties).FullName);
    static readonly string activityAnalysisCountColumn = "activityanalysiscount";
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MONITORED_MACHINE_ANALYSIS_STATUS,
                          new Column (activityAnalysisCountColumn, DbType.Int32, ColumnProperty.NotNull, 0));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MONITORED_MACHINE_ANALYSIS_STATUS,
                             activityAnalysisCountColumn);
    }
  }
}
