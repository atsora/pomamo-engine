// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 519: it was for Fanuc but too long
  /// </summary>
  [Migration(519)]
  public class Nothing: MigrationExt
  {
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
