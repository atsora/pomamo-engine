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
  /// Migration 543: add a pluginflag column to the plugin table
  /// </summary>
  [Migration (543)]
  public class AddPluginFlag : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddPluginFlag).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.PLUGIN,
        new Column (TableName.PLUGIN + "flag", DbType.Int32));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.PLUGIN, TableName.PLUGIN + "flag");
    }
  }
}
