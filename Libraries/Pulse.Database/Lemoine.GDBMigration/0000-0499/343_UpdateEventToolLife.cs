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
  /// Migration 343:
  /// </summary>
  [Migration(343)]
  public class UpdateEventToolLife: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UpdateEventToolLife).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Remove tool changed events
      RemoveEventsType2();
      
      // oldtoolnumber, newtoolnumber => toolnumber
      ChangeIntoSingle("toolnumber", true);
      
      // magazine => oldmagazine, newmagazine
      ChangeIntoOldNew("magazine", false);
      
      // pot => oldpot, newpot
      ChangeIntoOldNew("pot", false);
      
      // Delete the columns "initial"
      Database.RemoveColumn(TableName.TOOL_LIFE, TableName.TOOL_LIFE + "initial");
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "oldinitial");
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "newinitial");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Remove tool moved events
      RemoveEventsType2();
      
      // toolnumber => oldtoolnumber, newtoolnumber
      ChangeIntoOldNew("toolnumber", true);
      
      // oldmagazine, newmagazine => magazine
      ChangeIntoSingle("magazine", false);
      
      // oldpot, newpot => pot
      ChangeIntoSingle("pot", false);
      
      // Restore the columns "initial"
      Database.AddColumn(TableName.TOOL_LIFE, TableName.TOOL_LIFE + "initial", DbType.Double);
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "oldinitial", DbType.Double);
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "newinitial", DbType.Double);
    }
    
    void ChangeIntoOldNew(string name, bool notNull)
    {
      string oldName = "old" + name;
      string newName = "new" + name;
      
      // Create the "old" and "new" columns
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + oldName,
                         DbType.Int32, 4, notNull ? ColumnProperty.NotNull : ColumnProperty.None, 0);
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + newName,
                         DbType.Int32, 4, notNull ? ColumnProperty.NotNull : ColumnProperty.None, 0);
      
      // Copy the merged column into the "old" and "new" columns
      Database.ExecuteNonQuery(string.Format("UPDATE {0} SET {1} = {2}",
                                             TableName.EVENT_TOOL_LIFE,
                                             TableName.EVENT_TOOL_LIFE + newName,
                                             TableName.EVENT_TOOL_LIFE + name));
      Database.ExecuteNonQuery(string.Format("UPDATE {0} SET {1} = {2}",
                                             TableName.EVENT_TOOL_LIFE,
                                             TableName.EVENT_TOOL_LIFE + oldName,
                                             TableName.EVENT_TOOL_LIFE + name));
      
      // Remove the merged column
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + name);
    }
    
    void ChangeIntoSingle(string name, bool notNull)
    {
      string oldName = "old" + name;
      string newName = "new" + name;
      
      // Create the merged column
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + name,
                         DbType.Int32, 4, notNull ? ColumnProperty.NotNull : ColumnProperty.None, 0);
      
      // Copy the "new" column into the merged column
      Database.ExecuteNonQuery(string.Format("UPDATE {0} SET {1} = {2}",
                                             TableName.EVENT_TOOL_LIFE,
                                             TableName.EVENT_TOOL_LIFE + name,
                                             TableName.EVENT_TOOL_LIFE + newName));
      
      // Delete the "old" and "new" columns
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + oldName);
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + newName);
    }
    
    void RemoveEventsType2()
    {
      Database.ExecuteNonQuery(string.Format("DELETE FROM {0} WHERE {1} = 2",
                                             TableName.EVENT_TOOL_LIFE,
                                             TableName.EVENT_TOOL_LIFE + "typeid"));
    }
  }
}
