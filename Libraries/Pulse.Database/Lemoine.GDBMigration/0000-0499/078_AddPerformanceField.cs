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
  /// Migration 078: Add a performance column in the monitoredmachine table
  /// </summary>
  [Migration(78)]
  public class AddPerformanceField: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddPerformanceField).FullName);
    
    static readonly string PERFORMANCE_FIELD_COLUMN = "performancefieldid";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MONITORED_MACHINE,
                          new Column (PERFORMANCE_FIELD_COLUMN, DbType.Int32));
      Database.GenerateForeignKey (TableName.MONITORED_MACHINE, PERFORMANCE_FIELD_COLUMN,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MONITORED_MACHINE_ANALYSIS_STATUS, PERFORMANCE_FIELD_COLUMN);
    }
  }
}
