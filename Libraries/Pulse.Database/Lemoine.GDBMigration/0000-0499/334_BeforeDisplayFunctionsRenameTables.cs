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
  /// Migration 334:
  /// </summary>
  [Migration(334)]
  public class BeforeDisplayFunctionsRenameTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BeforeDisplayFunctionsRenameTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RenameTable (TableName.LINE_OLD, TableName.LINE);
      Database.RenameTable (TableName.PATH_OLD, TableName.PATH);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RenameTable (TableName.LINE, TableName.LINE_OLD);
      Database.RenameTable (TableName.PATH, TableName.PATH_OLD);
    }
  }
}
