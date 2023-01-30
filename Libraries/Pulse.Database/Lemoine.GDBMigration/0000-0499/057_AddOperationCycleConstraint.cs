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
  /// Migration 057: Add some constraints on the operationcycle table:
  /// <item>begin or end must be known</item>
  /// <item>if both begin and end are present, begin is before end</item>
  /// </summary>
  [Migration(57)]
  public class AddOperationCycleConstraint: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationCycleConstraint).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddCheckConstraint ("operationcycle_begin_or_end",
                                   TableName.OPERATION_CYCLE,
                                   @"
operationcyclebegin IS NOT NULL
OR operationcycleend IS NOT NULL");
      Database.AddCheckConstraint ("operationcycle_begin_before_end",
                                   TableName.OPERATION_CYCLE,
                                   @"
operationcyclebegin IS NULL
OR operationcycleend IS NULL
OR operationcyclebegin <= operationcycleend");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveConstraint (TableName.OPERATION_CYCLE,
                                 "operationcycle_begin_before_end");
      Database.RemoveConstraint (TableName.OPERATION_CYCLE,
                                 "operationcycle_begin_or_end");
    }
  }
}
