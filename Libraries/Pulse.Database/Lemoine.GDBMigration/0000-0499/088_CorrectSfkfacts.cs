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
  /// Migration 88 (deprecated): Correct the sfkfacts table:
  /// <item>(removed) remove big opid values that sould not be here</item>
  /// </summary>
  [Migration(88)]
  public class CorrectSfkfacts: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CorrectSfkfacts).FullName);
    
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
