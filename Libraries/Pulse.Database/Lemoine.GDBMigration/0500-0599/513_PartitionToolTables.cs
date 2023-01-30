// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 513:
  /// </summary>
  [Migration(513)]
  public class PartitionToolTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PartitionToolTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      PartitionTable(TableName.TOOL_POSITION, TableName.MACHINE_MODULE);
      PartitionTable(TableName.TOOL_LIFE, TableName.MACHINE_MODULE);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Nothing: 511, 512 and 513 fix tool tables
    }
  }
}
