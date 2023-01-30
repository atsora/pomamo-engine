// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 231:
  /// </summary>
  [Migration(231)]
  public class DropConstraintBetweenCyclesUniqueBegin: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DropConstraintBetweenCyclesUniqueBegin).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveUniqueConstraint(TableName.BETWEEN_CYCLES,
                             ColumnName.MACHINE_ID,
                             TableName.BETWEEN_CYCLES + "begin");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      AddUniqueConstraint(TableName.BETWEEN_CYCLES,
                          ColumnName.MACHINE_ID,
                          TableName.BETWEEN_CYCLES + "begin");
    }
  }
}
