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
  /// Migration 148: Remove some deprecated fields:
  /// <item>Cutting feedrate</item>
  /// </summary>
  [Migration(148)]
  public class RemoveDeprecatedFields: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveDeprecatedFields).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM field WHERE fieldid=107");
      Database.ExecuteNonQuery (@"DELETE FROM field WHERE fieldid=112");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
