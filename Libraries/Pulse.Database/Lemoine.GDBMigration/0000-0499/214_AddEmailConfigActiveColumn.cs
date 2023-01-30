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
  /// Migration 214: add a column active (true by default) to emailconfig
  /// </summary>
  [Migration(214)]
  public class AddEmailConfigActiveColumn: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEmailConfigActiveColumn).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.EMAIL_CONFIG,
                          new Column (TableName.EMAIL_CONFIG + "active", DbType.Boolean, ColumnProperty.NotNull, "TRUE"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                            TableName.EMAIL_CONFIG + "active");
    }
  }
}
