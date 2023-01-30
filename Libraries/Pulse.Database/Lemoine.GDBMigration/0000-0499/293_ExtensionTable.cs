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
  /// Migration 293: add the tables "plugin" and "extension"
  /// </summary>
  [Migration(293)]
  public class ExtensionTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ExtensionTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Table plugin
      Database.AddTable(TableName.PLUGIN,
                        new Column(ColumnName.PLUGIN_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.PLUGIN + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.PLUGIN + "name", DbType.String, ColumnProperty.NotNull));
      AddUniqueConstraint(TableName.PLUGIN, TableName.PLUGIN + "name");
      
      // Table extension
      Database.AddTable("extension",
                        new Column("extensionid", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column("extension" + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.PLUGIN_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column("extension" + "name", DbType.String, ColumnProperty.NotNull),
                        new Column("extension" + "parameters", DbType.String));
      Database.GenerateForeignKey("extension", "extensionid",
                                  TableName.PLUGIN, ColumnName.PLUGIN_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      Database.RemoveTable("extension");
      Database.RemoveTable(TableName.PLUGIN);
    }
  }
}
