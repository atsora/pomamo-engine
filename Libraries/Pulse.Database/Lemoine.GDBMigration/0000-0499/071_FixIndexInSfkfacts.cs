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
  /// Migration 071:
  /// <item>(removed) Fix an index in sfkfacts that must not be unique</item>
  /// <item>Remove the trigger machineprocesstype</item>
  /// </summary>
  [Migration(71)]
  public class FixIndexInSfkfacts: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixIndexInSfkfacts).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS machineprocesstype() CASCADE;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
