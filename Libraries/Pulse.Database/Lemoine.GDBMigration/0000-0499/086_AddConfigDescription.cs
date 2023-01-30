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
  /// Migration 086: Add a column description into the table config
  /// </summary>
  [Migration(86)]
  public class AddConfigDescription: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddConfigDescription).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.CONFIG,
                          new Column (TableName.CONFIG + "description", DbType.String, ColumnProperty.Null));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CONFIG, TableName.CONFIG + "description");
    }
  }
}
