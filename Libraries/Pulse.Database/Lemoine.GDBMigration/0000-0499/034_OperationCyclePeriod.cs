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
  /// Migration 034: Add the OperationCyclePeriod modification table
  /// </summary>
  [Migration(34)]
  public class OperationCyclePeriod: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationCyclePeriod).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.OPERATION_CYCLE_PERIOD)) {
        AddOperationCyclePeriodTable ();
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.OPERATION_CYCLE_PERIOD)) {
        Database.RemoveTable (TableName.OPERATION_CYCLE_PERIOD);
      }
    }
    
    /// <summary>
    /// Add the modification table OperationCyclePeriod
    /// </summary>
    private void AddOperationCyclePeriodTable ()
    {
      Database.AddTable (TableName.OPERATION_CYCLE_PERIOD,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("OperationCyclePeriodBegin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("OperationCyclePeriodEnd", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("FullCycleNumber", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column ("PartialCycleNumber", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column ("TotalCycleTime", DbType.Int32, ColumnProperty.NotNull),
                         new Column ("TotalLoadingTime", DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_PERIOD, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_PERIOD, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.OPERATION_CYCLE_PERIOD));
    }
  }
}
