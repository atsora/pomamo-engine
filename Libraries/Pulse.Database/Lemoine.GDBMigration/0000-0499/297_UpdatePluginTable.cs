// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using System.Data;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 297: parameters are centralized for a plugin
  /// The extension table is not needed anymore, parameters being common to all extensions within a plugin
  /// </summary>
  [Migration(297)]
  public class UpdatePluginTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UpdatePluginTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Remove the table extension
      Database.RemoveTable("extension");
      
      // Add columns to the plugin table
      Database.AddColumn(TableName.PLUGIN, TableName.PLUGIN + "parameters", DbType.String);
      Database.AddColumn(TableName.PLUGIN,
                         new Column(TableName.PLUGIN + "activated", DbType.Boolean,
                                    ColumnProperty.NotNull, true));
      Database.AddColumn(TableName.PLUGIN,
                         new Column(TableName.PLUGIN + "numversion", DbType.Int32,
                                    ColumnProperty.NotNull, 1));
      
      // Create schema "plugins"
      Database.ExecuteNonQuery("CREATE SCHEMA IF NOT EXISTS plugins;");
      
      // Add privilege to user "reportV2" on the schema and future tables in it
      // cf: files "reportRightsV2.sql", "reportRoleV2.sql"
      Database.ExecuteNonQuery(@"
DO
$body$
BEGIN
   IF NOT EXISTS (
      SELECT *
      FROM   pg_catalog.pg_user
      WHERE  usename = 'reportv2') THEN
         CREATE ROLE reportv2 LOGIN PASSWORD 'PulseReportV2'
          NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION;
   END IF;
END
$body$;

GRANT SELECT
ON ALL TABLES IN SCHEMA plugins, public
TO reportv2;

GRANT EXECUTE
ON ALL FUNCTIONS IN SCHEMA plugins, public
TO reportv2;

GRANT USAGE
ON SCHEMA plugins, public
TO reportv2;
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Remove columns from the plugin table
      Database.RemoveColumn(TableName.PLUGIN, TableName.PLUGIN + "numversion");
      Database.RemoveColumn(TableName.PLUGIN, TableName.PLUGIN + "activated");
      Database.RemoveColumn(TableName.PLUGIN, TableName.PLUGIN + "parameters");
      
      // Restore the table extension
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
  }
}
