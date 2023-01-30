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
  /// Migration 506:
  /// </summary>
  [Migration(506)]
  public class SetTaskOperationIdNullable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SetTaskOperationIdNullable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      DropNotNull (TableName.TASK_FULL, ColumnName.OPERATION_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      SetNotNull (TableName.TASK_FULL, ColumnName.OPERATION_ID);
    }
  }
}
