// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Migrator.Framework;
using System.Data;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 289: Add the column "editor" in the "emailconfig" table
  /// freefilter will be then be used for filtering
  /// the editor information being used to gather rows within LemSettings
  /// </summary>
  [Migration(289)]
  public class AddEditorColumnInEmailConfig: MigrationExt
  {
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Create editor column
      Database.AddColumn(TableName.EMAIL_CONFIG, TableName.EMAIL_CONFIG + "editor", DbType.String);
    
      // Move some content from the free filter column
      string query = string.Format("UPDATE {0} " +
                                   "SET {1} = '{2}', {3} = '' " +
                                   "WHERE {3} = '{2}' OR {3} = '{4}'",
                                   TableName.EMAIL_CONFIG,
                                   TableName.EMAIL_CONFIG + "editor",
                                   "LemSettings - Email alert configurator",
                                   TableName.EMAIL_CONFIG + "freefilter",
                                   "AlertConfigGUI");
      Database.ExecuteNonQuery(query);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Move content to the free filter column
      string query = string.Format("UPDATE {0} " +
                                   "SET {1} = {2} " +
                                   "WHERE {2} != ''",
                                   TableName.EMAIL_CONFIG,
                                   TableName.EMAIL_CONFIG + "freefilter",
                                   TableName.EMAIL_CONFIG + "editor");
      Database.ExecuteNonQuery(query);
      
      // Remove editor column
      Database.RemoveColumn(TableName.EMAIL_CONFIG, TableName.EMAIL_CONFIG + "editor");
    }
  }
}
