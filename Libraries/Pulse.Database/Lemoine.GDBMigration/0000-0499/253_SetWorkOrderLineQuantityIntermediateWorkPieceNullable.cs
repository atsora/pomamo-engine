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
  /// Migration 253: Set workorderlinequantity.intermediateworkpieceid nullable for NHibernate
  /// </summary>
  [Migration(253)]
  public class SetWorkOrderLineQuantityIntermediateWorkPieceNullable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SetWorkOrderLineQuantityIntermediateWorkPieceNullable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.WORK_ORDER_LINE_QUANTITY,
                                               ColumnName.INTERMEDIATE_WORK_PIECE_ID));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               TableName.WORK_ORDER_LINE_QUANTITY,
                                               ColumnName.INTERMEDIATE_WORK_PIECE_ID));
    }
  }
}
