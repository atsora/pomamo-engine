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
  /// Migration 152: Add the cyclecountsummary analysis table
  /// </summary>
  [Migration(152)]
  public class AddCycleCountSummary: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCycleCountSummary).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Now part of Lemoine.Plugin.CycleCountSummary
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Now part of Lemoine.Plugin.CycleCountSummary
    }
  }
}
