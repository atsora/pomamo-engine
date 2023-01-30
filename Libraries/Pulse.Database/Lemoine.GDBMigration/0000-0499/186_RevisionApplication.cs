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
  /// Migration 186: add an application column to the revision table
  /// </summary>
  [Migration(186)]
  public class RevisionApplication: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RevisionApplication).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.REVISION,
                          new Column (TableName.REVISION + "application", DbType.String, 32, ColumnProperty.Null));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REVISION,
                             TableName.REVISION + "application");
    }
  }
}
