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
  /// Migration 112: add priority column to Modification table
  /// </summary>
  [Migration(112)]
  public class AddPriorityColumnToModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddPriorityColumnToModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MODIFICATION,
                          new Column (ColumnName.MODIFICATION_PRIORITY, DbType.Int32, ColumnProperty.NotNull, 100));
      
      AddIndex (TableName.MODIFICATION,
                ColumnName.MODIFICATION_ID,
                ColumnName.MODIFICATION_PRIORITY);
      
      AddIndex (TableName.MODIFICATION,
                ColumnName.MODIFICATION_DATETIME,
                ColumnName.MODIFICATION_PRIORITY);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MODIFICATION,
                             "modificationpriority");
    }
  }
}
