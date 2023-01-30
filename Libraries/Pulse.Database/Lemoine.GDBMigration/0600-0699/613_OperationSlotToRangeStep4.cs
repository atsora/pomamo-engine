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
  /// Migration 613:
  /// </summary>
  [Migration(613)]
  public class OperationSlotToRangeStep4: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotToRangeStep4).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNoOverlapConstraint (TableName.OPERATION_SLOT,
                              TableName.OPERATION_SLOT + "datetimerange",
                              ColumnName.MACHINE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNoOverlapConstraint (TableName.OPERATION_SLOT);
    }
  }
}
