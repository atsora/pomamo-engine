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
  /// Migration 329: add a column name to package plugin association name
  /// </summary>
  [Migration(329)]
  public class PackagePluginAssociationName: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PackagePluginAssociationName).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.PACKAGE_PLUGIN_ASSOCIATION,
                          new Column (TableName.PACKAGE_PLUGIN_ASSOCIATION + "name", DbType.String));
      MakeColumnCaseInsensitive (TableName.PACKAGE_PLUGIN_ASSOCIATION,
                                 TableName.PACKAGE_PLUGIN_ASSOCIATION + "name");
      RemoveUniqueConstraint (TableName.PACKAGE_PLUGIN_ASSOCIATION, ColumnName.PACKAGE_ID, ColumnName.PLUGIN_ID);
      AddUniqueConstraint (TableName.PACKAGE_PLUGIN_ASSOCIATION, ColumnName.PACKAGE_ID, ColumnName.PLUGIN_ID,
                           TableName.PACKAGE_PLUGIN_ASSOCIATION + "name");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.PACKAGE_PLUGIN_ASSOCIATION,
                             TableName.PACKAGE_PLUGIN_ASSOCIATION + "name");
      AddUniqueConstraint(TableName.PACKAGE_PLUGIN_ASSOCIATION, ColumnName.PACKAGE_ID, ColumnName.PLUGIN_ID);
    }
  }
}
