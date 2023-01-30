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
  /// Migration 1108: Partition the sequencemilestone table 
  /// </summary>
  [Migration (1108)]
  public class PartitionSequenceMilestone : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PartitionSequenceMilestone).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      PartitionTable (TableName.SEQUENCE_MILESTONE, TableName.MACHINE_MODULE);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
