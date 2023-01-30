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
  /// Migration 139: add a new summary analysis that returns the number of cycles for each
  /// <item>machine</item>
  /// <item>day</item>
  /// <item>work order</item>
  /// <item>component</item>
  /// <item>operation</item>
  /// <item>duration offset</item>
  /// </summary>
  [Migration(139)]
  public class CycleDurationSummary: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CycleDurationSummary).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // This is now part of a plugin in version 8.0
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
