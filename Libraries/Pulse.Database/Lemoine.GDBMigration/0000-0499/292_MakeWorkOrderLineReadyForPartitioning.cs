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
  /// Migration 292:
  /// </summary>
  [Migration(292)]
  public class MakeWorkOrderLineReadyForPartitioning: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MakeWorkOrderLineReadyForPartitioning).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.WORK_ORDER_LINE_QUANTITY,
                          new Column (ColumnName.LINE_ID, DbType.Int32));
      Database.ExecuteNonQuery (@"UPDATE workorderlinequantity
SET lineid=workorderline.lineid
FROM workorderline
WHERE workorderline.workorderlineid=workorderlinequantity.workorderlineid");
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               TableName.WORK_ORDER_LINE_QUANTITY,
                                               ColumnName.LINE_ID));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.WORK_ORDER_LINE_QUANTITY, ColumnName.LINE_ID);
    }
  }
}
