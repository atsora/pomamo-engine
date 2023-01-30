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
  /// Migration 327: add a parent column in machinemode table
  /// 
  /// And make machinemoderunning NULLable
  /// </summary>
  [Migration(327)]
  public class AddMachineModeParent: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineModeParent).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODE,
                          new Column ("parent" + ColumnName.MACHINE_MODE_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.MACHINE_MODE, "parent" + ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} DROP NOT NULL",
                                               TableName.MACHINE_MODE,
                                               "running"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemoderunning=FALSE
WHERE machinemoderunning IS NULL");
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.MACHINE_MODE,
                                               "running"));
      
      Database.RemoveColumn (TableName.MACHINE_MODE, "parent" + ColumnName.MACHINE_MODE_ID);
    }
  }
}
