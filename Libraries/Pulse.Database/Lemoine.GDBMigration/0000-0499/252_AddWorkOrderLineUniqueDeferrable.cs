// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 252: Make the unique constraint on workorderline deferrable
  /// </summary>
  [Migration(252)]
  public class AddWorkOrderLineUniqueDeferrable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddWorkOrderLineUniqueDeferrable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveConstraint (TableName.WORK_ORDER_LINE,
                                 TableName.WORK_ORDER_LINE + "_unique");
      AddUniqueConstraint (TableName.WORK_ORDER_LINE,
                           ColumnName.LINE_ID,
                           ColumnName.WORK_ORDER_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUniqueConstraint (TableName.WORK_ORDER_LINE,
                              ColumnName.LINE_ID,
                              ColumnName.WORK_ORDER_ID);
      AddNamedUniqueConstraint (TableName.WORK_ORDER_LINE + "_unique",
                             TableName.WORK_ORDER_LINE,
                             ColumnName.WORK_ORDER_ID,
                             ColumnName.LINE_ID);
    }
  }
}
