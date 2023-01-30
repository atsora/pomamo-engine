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
  /// Migration 039:
  /// <item>Add an analysis column in auto-process table</item>
  /// </summary>
  [Migration(39)]
  public class AutoProcessAnalysis: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AutoProcessAnalysis).FullName);
    
    static readonly string ANALYSIS_COLUMN = "autoprocessanalysis";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OLD_AUTO_SEQUENCE,
                          new Column (ANALYSIS_COLUMN, DbType.DateTime));
      Database.ExecuteNonQuery (@"UPDATE autoprocess 
SET autoprocessanalysis=autoprocessactivityend 
WHERE autoprocessanalysis IS NULL AND autoprocessactivityend IS NOT NULL");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OLD_AUTO_SEQUENCE,
                             ANALYSIS_COLUMN);
    }
  }
}
