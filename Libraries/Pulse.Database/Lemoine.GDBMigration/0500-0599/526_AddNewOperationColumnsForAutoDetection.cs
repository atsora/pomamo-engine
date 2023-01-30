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
  /// Migration 526:
  /// </summary>
  [Migration(526)]
  public class AddNewOperationColumnsForAutoDetection: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNewOperationColumnsForAutoDetection).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // - operation.machinefilterid
      Database.AddColumn (TableName.OPERATION,
                          new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.OPERATION, ColumnName.MACHINE_FILTER_ID,
                                   TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.OPERATION, ColumnName.MACHINE_FILTER_ID);
      
      // - operation.operationlock
      Database.AddColumn (TableName.OPERATION,
                          new Column (TableName.OPERATION + "lock", DbType.Boolean, "TRUE"));
      Database.ExecuteNonQuery ("UPDATE operation SET operationlock=TRUE");
      SetNotNull (TableName.OPERATION, TableName.OPERATION + "lock");
      
      // - sequence.operationstep
      Database.AddColumn (TableName.SEQUENCE,
                          new Column (TableName.SEQUENCE + "operationstep", DbType.Int32));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.SEQUENCE,
                             TableName.SEQUENCE + "operationstep");
      Database.RemoveColumn (TableName.OPERATION,
                             TableName.OPERATION + "lock");
      Database.RemoveColumn (TableName.OPERATION,
                             ColumnName.MACHINE_FILTER_ID);
    }
  }
}
