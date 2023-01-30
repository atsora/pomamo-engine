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
  /// Migration 254: rework the indexes of the modification table
  /// </summary>
  [Migration(254)]
  public class ReworkModificationIndexes: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReworkModificationIndexes).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.MODIFICATION, "datetime");
      
      AddIndex (TableName.MODIFICATION,
                "parentmodificationid");
      
      RemoveIndex (TableName.MODIFICATION,
                   ColumnName.MODIFICATION_ID,
                   ColumnName.MODIFICATION_PRIORITY);
      AddIndexCondition (TableName.MODIFICATION,
                         "parentmodificationid IS NULL",
                         ColumnName.MODIFICATION_ID,
                         ColumnName.MODIFICATION_PRIORITY);
      
      RemoveIndex (TableName.MODIFICATION,
                   ColumnName.MODIFICATION_DATETIME,
                   ColumnName.MODIFICATION_PRIORITY);
      AddIndexCondition (TableName.MODIFICATION,
                         "parentmodificationid IS NULL",
                         ColumnName.MODIFICATION_DATETIME,
                         ColumnName.MODIFICATION_PRIORITY);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("CREATE INDEX modification_datetime_idx " +
                                "ON modification (modificationdatetime);");

      RemoveIndex (TableName.MODIFICATION,
                   "parentmodificationid");
      
      RemoveIndex (TableName.MODIFICATION,
                   ColumnName.MODIFICATION_ID,
                   ColumnName.MODIFICATION_PRIORITY);
      AddIndex (TableName.MODIFICATION,
                ColumnName.MODIFICATION_ID,
                ColumnName.MODIFICATION_PRIORITY);
      
      RemoveIndex (TableName.MODIFICATION,
                   ColumnName.MODIFICATION_DATETIME,
                   ColumnName.MODIFICATION_PRIORITY);
      AddIndex (TableName.MODIFICATION,
                ColumnName.MODIFICATION_DATETIME,
                ColumnName.MODIFICATION_PRIORITY);
    }
  }
}
