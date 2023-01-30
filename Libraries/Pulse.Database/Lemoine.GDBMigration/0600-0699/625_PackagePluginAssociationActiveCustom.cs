// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 625: add 2 new columns: active/custom to packagepluginassociation 
  /// </summary>
  [Migration (625)]
  public class PackagePluginAssociationActiveCustom : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PackagePluginAssociationActiveCustom).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.PACKAGE_PLUGIN_ASSOCIATION, TableName.PACKAGE_PLUGIN_ASSOCIATION + "active", DbType.Boolean);
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET {0}active=TRUE
", TableName.PACKAGE_PLUGIN_ASSOCIATION));
      SetNotNull (TableName.PACKAGE_PLUGIN_ASSOCIATION, TableName.PACKAGE_PLUGIN_ASSOCIATION + "active");

      Database.AddColumn (TableName.PACKAGE_PLUGIN_ASSOCIATION, TableName.PACKAGE_PLUGIN_ASSOCIATION + "custom", DbType.Boolean);
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET {0}custom=FALSE
", TableName.PACKAGE_PLUGIN_ASSOCIATION));
      SetNotNull (TableName.PACKAGE_PLUGIN_ASSOCIATION, TableName.PACKAGE_PLUGIN_ASSOCIATION + "custom");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.PACKAGE_PLUGIN_ASSOCIATION, TableName.PACKAGE_PLUGIN_ASSOCIATION + "active");
      Database.RemoveColumn (TableName.PACKAGE_PLUGIN_ASSOCIATION, TableName.PACKAGE_PLUGIN_ASSOCIATION + "custom");
    }
  }
}
