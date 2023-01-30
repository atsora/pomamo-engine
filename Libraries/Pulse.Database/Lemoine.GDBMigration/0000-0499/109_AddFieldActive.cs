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
  /// Migration 109: Add a column Active to the field table
  /// </summary>
  [Migration(109)]
  public class AddFieldActive: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddFieldActive).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.FIELD,
                          new Column (TableName.FIELD + "active", DbType.Boolean, ColumnProperty.NotNull, true));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.FIELD,
                             TableName.FIELD + "active");
    }
  }
}
