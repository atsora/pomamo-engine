// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1100: add a column sequencemilestone in machinemoduledetection
  /// </summary>
  [Migration (1100)]
  public class MachineModuleDetectionSequenceMilestone : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineModuleDetectionSequenceMilestone).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODULE_DETECTION,
        new Column ("sequencemilestone", DbType.Int32));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE_DETECTION, "sequencemilestone");
    }
  }
}
