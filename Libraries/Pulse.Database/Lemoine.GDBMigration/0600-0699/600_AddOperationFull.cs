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
  /// Migration 600:
  /// </summary>
  [Migration(600)]
  public class AddOperationCycleMakesPart: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationCycleMakesPart).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
DROP SCHEMA IF EXISTS reportv2 CASCADE;
");
      Database.ExecuteNonQuery (@"
DROP SCHEMA IF EXISTS extern CASCADE;
");

      // Retrofit that to allow new default values
      SetMinSequence (TableName.REASON_GROUP, ColumnName.REASON_GROUP_ID, 100);
      SetMinSequence (TableName.REASON, ColumnName.REASON_ID, 100);
      
      Database.AddColumn (TableName.OPERATION_CYCLE,
                          new Column (TableName.OPERATION_CYCLE + "full", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "full");
    }
  }
}
