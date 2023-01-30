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
  /// Migration 265:
  /// </summary>
  [Migration(265)]
  public class AddAnalysisStatusCategory: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddAnalysisStatusCategory).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.ANALYSIS_STATUS,
                          new Column (TableName.ANALYSIS_STATUS + "category", DbType.String, 256));
      AddCategory (0, "New");
      AddCategory (1, "Pending");
      AddCategory (3, "Completed");
      AddCategory (4, "Error");
      AddCategory (5, "Obsolete");
      AddCategory (6, "Delete");
      AddCategory (7, "Error");
      AddCategory (8, "Error");
      AddCategory (9, "InProgress");
      AddCategory (10, "InProgress");
      AddCategory (11, "InProgress");
      AddCategory (12, "Cancel");
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               TableName.ANALYSIS_STATUS,
                                               TableName.ANALYSIS_STATUS + "category"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.ANALYSIS_STATUS,
                            TableName.ANALYSIS_STATUS + "category");
    }
    
    void AddCategory (int id, string category)
    {
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}category='{1}'
WHERE {0}id={2}",
                                               TableName.ANALYSIS_STATUS, category, id));
    }
  }
}
