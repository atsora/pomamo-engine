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
  /// Migration 295:
  /// </summary>
  [Migration(295)]
  public class WorkOrderLineQuantityForeignKey: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderLineQuantityForeignKey).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      try {
        Database.GenerateForeignKey (TableName.WORK_ORDER_LINE_QUANTITY, ColumnName.LINE_ID,
                                     TableName.LINE_OLD, ColumnName.LINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
      catch (Exception) {
        log.DebugFormat ("Up: " +
                         "the foreign key is already defined");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
