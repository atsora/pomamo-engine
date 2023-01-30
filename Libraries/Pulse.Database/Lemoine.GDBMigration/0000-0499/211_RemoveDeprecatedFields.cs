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
  /// Migration 211: remove two deprecated fields from the field table:
  /// <item>107: cutting feedrate</item>
  /// <item>112: US cutting feedrate</item>
  /// </summary>
  [Migration(211)]
  public class RemoveDeprecatedFields2: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveDeprecatedFields2).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM field WHERE fieldid IN (112, 107)");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
