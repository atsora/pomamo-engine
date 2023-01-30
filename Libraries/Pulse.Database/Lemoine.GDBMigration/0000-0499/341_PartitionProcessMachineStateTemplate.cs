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
  /// Migration 341:
  /// </summary>
  [Migration(341)]
  public class PartitionProcessMachineStateTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PartitionProcessMachineStateTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (IsPartitioned (TableName.MACHINE_MODIFICATION)
          && !IsPartitioned (TableName.PROCESS_MACHINE_STATE_TEMPLATE)) {
        PartitionTable (TableName.PROCESS_MACHINE_STATE_TEMPLATE, TableName.MACHINE);
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
