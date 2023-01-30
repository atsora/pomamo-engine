// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 509: add a column "properties" in toollife position for storing various data
  /// "cuttercompensation", "lengthcompensation" and "geometryunitid" will be inside
  /// This is now empty since migrations 511 and 512 create the tool tables
  /// </summary>
  [Migration(509)]
  public class ToolPositionProperties: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof(ToolPositionProperties).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up() {}
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down() {}
  }
}
