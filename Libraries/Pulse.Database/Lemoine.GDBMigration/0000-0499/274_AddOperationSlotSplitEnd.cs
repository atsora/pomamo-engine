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
  /// Migration 274: add a column in machine status for operationslotsplitend: obsolete
  /// Replaced by migration 276
  /// </summary>
  [Migration(274)]
  public class AddOperationSlotSplitEnd: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationSlotSplitEnd).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
