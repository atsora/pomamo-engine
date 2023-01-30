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
  /// Migration 299: add the table "package" and the table "packagepluginassociation", which is
  /// an association table between "package" and "plugin":
  /// - a package is made of one or several plugins,
  /// - a plugin is used by one or several packages.
  /// </summary>
  [Migration(299)]
  public class AddPackageTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddPackageTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      ModifyPlugin();
      CreatePackage();
      CreatePackagePluginAssociation();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      DeletePackagePluginAssociation();
      DeletePackage();
      RestorePlugin();
    }
    
    void ModifyPlugin()
    {
      // Clear data
      //Database.ExecuteNonQuery("DELETE FROM " + TableName.PLUGIN + ";");
      
      // Rename column "name" into "identifyingname"
      Database.AddColumn(TableName.PLUGIN, new Column(TableName.PLUGIN + "identifyingname", DbType.String));
      Database.ExecuteNonQuery("UPDATE " + TableName.PLUGIN +
                              " SET " + TableName.PLUGIN + "identifyingname = " +
                              TableName.PLUGIN + "name");
      AddUniqueConstraint(TableName.PLUGIN, TableName.PLUGIN + "identifyingname");
      Database.RemoveColumn(TableName.PLUGIN, TableName.PLUGIN + "name");
      
      // Remove "activated" and "parameters" columns from the table plugin
      Database.RemoveColumn(TableName.PLUGIN, TableName.PLUGIN + "activated");
      Database.RemoveColumn(TableName.PLUGIN, TableName.PLUGIN + "parameters");
    }
    
    void RestorePlugin()
    {
      // Clear data
      //Database.ExecuteNonQuery("DELETE FROM " + TableName.PLUGIN + ";");
      
      // Rename column "identifyingname" into "name"
      Database.AddColumn(TableName.PLUGIN, new Column(TableName.PLUGIN + "name", DbType.String));
      Database.ExecuteNonQuery("UPDATE " + TableName.PLUGIN +
                              " SET " + TableName.PLUGIN + "name = " +
                              TableName.PLUGIN + "identifyingname");
      AddUniqueConstraint(TableName.PLUGIN, TableName.PLUGIN + "name");
      Database.RemoveColumn(TableName.PLUGIN, TableName.PLUGIN + "identifyingname");
      
      // Add columns "activated", "parameters"
      Database.AddColumn(TableName.PLUGIN, new Column(TableName.PLUGIN + "activated", DbType.Boolean, ColumnProperty.NotNull, true));
      Database.AddColumn(TableName.PLUGIN, new Column(TableName.PLUGIN + "parameters", DbType.String));
    }
    
    void CreatePackage()
    {
      // Creation of the table package
      Database.AddTable(TableName.PACKAGE,
                        new Column(ColumnName.PACKAGE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.PACKAGE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.PACKAGE + "name", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.PACKAGE + "identifyingname", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.PACKAGE + "numversion", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.PACKAGE + "activated", DbType.Boolean, ColumnProperty.NotNull, true),
                        new Column(TableName.PACKAGE + "details", DbType.String, ColumnProperty.NotNull));
      AddUniqueConstraint(TableName.PACKAGE, TableName.PACKAGE + "identifyingname");
    }
    
    void DeletePackage()
    {
      // Destruction of the table package
      Database.RemoveTable(TableName.PACKAGE);
    }
    
    void CreatePackagePluginAssociation()
    {
      // Creation of the table "packagepluginassociation" which links plugins to packages
      Database.AddTable(TableName.PACKAGE_PLUGIN_ASSOCIATION,
                        new Column(ColumnName.PACKAGE_PLUGIN_ASSOCIATION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.PACKAGE_PLUGIN_ASSOCIATION + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.PACKAGE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(ColumnName.PLUGIN_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.PACKAGE_PLUGIN_ASSOCIATION + "parameters", DbType.String, ColumnProperty.NotNull));
      AddUniqueConstraint(TableName.PACKAGE_PLUGIN_ASSOCIATION, ColumnName.PACKAGE_ID, ColumnName.PLUGIN_ID);
      
      // Unlimited number of characters
      Database.ExecuteNonQuery("ALTER TABLE " + TableName.PACKAGE_PLUGIN_ASSOCIATION +
                              " ALTER COLUMN " + TableName.PACKAGE_PLUGIN_ASSOCIATION + "parameters" +
                              " SET DATA TYPE TEXT;");
      
      // Foreign keys to the tables "package" and "plugin"
      Database.GenerateForeignKey(TableName.PACKAGE_PLUGIN_ASSOCIATION, ColumnName.PACKAGE_ID,
                                  TableName.PACKAGE, ColumnName.PACKAGE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.PACKAGE_PLUGIN_ASSOCIATION, ColumnName.PLUGIN_ID,
                                  TableName.PLUGIN, ColumnName.PLUGIN_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void DeletePackagePluginAssociation()
    {
      // Destruction of the table "packagepluginassociation"
      Database.RemoveTable(TableName.PACKAGE_PLUGIN_ASSOCIATION);
    }
  }
}
