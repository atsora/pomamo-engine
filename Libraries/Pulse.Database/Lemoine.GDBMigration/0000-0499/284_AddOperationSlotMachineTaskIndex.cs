// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 284:
  /// </summary>
  [Migration(284)]
  public class AddOperationSlotMachineTaskIndex: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationSlotMachineTaskIndex).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                ColumnName.TASK_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex(TableName.OPERATION_SLOT,
                  ColumnName.MACHINE_ID,
                  ColumnName.TASK_ID);
    }
  }
}
