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
  /// Migration 609:
  /// </summary>
  [Migration(609)]
  public class ReasonSlotToRangeStep4: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSlotToRangeStep4).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNoOverlapConstraint (TableName.REASON_SLOT,
                              TableName.REASON_SLOT + "datetimerange",
                              ColumnName.MACHINE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNoOverlapConstraint (TableName.REASON_SLOT);
    }
  }
}
