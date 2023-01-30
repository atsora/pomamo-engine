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
  /// Migration 120:
  /// add "operationcyclestatus" column in operation cycle (status of bound estimates)
  /// </summary>
  [Migration(120)]
  public class AddOperationCycleStatusColumn: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationCycleStatusColumn).FullName);        
    static readonly string columnName = TableName.OPERATION_CYCLE + "status";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION_CYCLE,
                          new Column (columnName, DbType.Int32));
      
      AddIndex (TableName.OPERATION_CYCLE,
                columnName);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_CYCLE, columnName);      
    }
  }
}
