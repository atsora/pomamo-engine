// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using System.Data;
using Migrator.Framework;
using Lemoine.GDBMigration;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    #region Members
    TransformationProviderExt m_database = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Maintenance actions"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "This plugin adds the 'maintenance actions' feature, to allow the maintenance guy to better manage the maintenance of the machines";
      }
    }

    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.Config | PluginFlag.Web | PluginFlag.NHibernateExtension;
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 2; } }

    TransformationProviderExt Database
    {
      get
      {
        if (null == m_database) {
          m_database = new TransformationProviderExt ();
        }
        return m_database;
      }
    }

    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Methods
    /// <summary>
    /// Install from a specific version
    /// (create or update tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    /// <param name="version"></param>
    protected override void InstallVersion (int version)
    {
      switch (version) {
        case 1: // First installation
          Install1 ();
          break;
        case 2:
          Install2 ();
          break;
        default:
          throw new InvalidOperationException ();
      }
    }

    #region Migration
    void Install1 ()
    {
      AddMaintenanceAction ();
      AddMaintenanceActionUpdate ();
    }

    void AddMaintenanceActionType ()
    {
      Database.AddTable (TableName.MAINTENANCE_ACTION_TYPE,
                         new Column (ColumnName.MAINTENANCE_ACTION_TYPE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MAINTENANCE_ACTION_TYPE + "name", DbType.String),
                         new Column (TableName.MAINTENANCE_ACTION_TYPE + "translationkey", DbType.String));
      Database.MakeColumnCaseInsensitive (TableName.MAINTENANCE_ACTION_TYPE, TableName.MAINTENANCE_ACTION_TYPE + "name");
      Database.AddConstraintNameTranslationKey (TableName.MAINTENANCE_ACTION_TYPE,
        TableName.MAINTENANCE_ACTION_TYPE + "name",
        TableName.MAINTENANCE_ACTION_TYPE + "translationkey");
      Database.ExecuteNonQuery (@"INSERT INTO translation(locale, translationkey, translationvalue)
SELECT '', 'MaintenanceActionPreventive', 'Preventive'
WHERE NOT EXISTS (SELECT 1 FROM translation WHERE locale='' AND translationkey='MaintenanceActionPreventive')");
      Database.ExecuteNonQuery (@"INSERT INTO translation(locale, translationkey, translationvalue)
SELECT '', 'MaintenanceActionCurative', 'Curative'
WHERE NOT EXISTS (SELECT 1 FROM translation WHERE locale='' AND translationkey='MaintenanceActionCurative')");
      Database.Insert (TableName.MAINTENANCE_ACTION_TYPE,
                       new string[] { ColumnName.MAINTENANCE_ACTION_TYPE_ID, TableName.MAINTENANCE_ACTION_TYPE + "translationkey" },
                       new string[] { "1", "MaintenanceActionPreventive" });
      Database.Insert (TableName.MAINTENANCE_ACTION_TYPE,
                       new string[] { ColumnName.MAINTENANCE_ACTION_TYPE_ID, TableName.MAINTENANCE_ACTION_TYPE + "translationkey" },
                       new string[] { "2", "MaintenanceActionCurative" });
    }

    void AddMaintenanceActionStatus ()
    {
      Database.AddTable (TableName.MAINTENANCE_ACTION_STATUS,
                         new Column (ColumnName.MAINTENANCE_ACTION_STATUS_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MAINTENANCE_ACTION_STATUS + "name", DbType.String),
                         new Column (TableName.MAINTENANCE_ACTION_STATUS + "translationkey", DbType.String));
      Database.MakeColumnCaseInsensitive (TableName.MAINTENANCE_ACTION_STATUS, TableName.MAINTENANCE_ACTION_STATUS + "name");
      Database.AddConstraintNameTranslationKey (TableName.MAINTENANCE_ACTION_STATUS,
        TableName.MAINTENANCE_ACTION_STATUS + "name",
        TableName.MAINTENANCE_ACTION_STATUS + "translationkey");
      Database.ExecuteNonQuery (@"INSERT INTO translation(locale, translationkey, translationvalue)
SELECT '', 'StatusOpen', 'Open'
WHERE NOT EXISTS (SELECT 1 FROM translation WHERE locale='' AND translationkey='StatusOpen')");
      Database.ExecuteNonQuery (@"INSERT INTO translation(locale, translationkey, translationvalue)
SELECT '', 'StatusCompleted', 'Completed'
WHERE NOT EXISTS (SELECT 1 FROM translation WHERE locale='' AND translationkey='StatusCompleted')");
      Database.Insert (TableName.MAINTENANCE_ACTION_STATUS,
                       new string[] { ColumnName.MAINTENANCE_ACTION_STATUS_ID, TableName.MAINTENANCE_ACTION_STATUS + "translationkey" },
                       new string[] { "1", "StatusOpen" });
      Database.Insert (TableName.MAINTENANCE_ACTION_STATUS,
                       new string[] { ColumnName.MAINTENANCE_ACTION_STATUS_ID, TableName.MAINTENANCE_ACTION_STATUS + "translationkey" },
                       new string[] { "2", "StatusCompleted" });
    }

    void AddMaintenanceAction ()
    {
      AddMaintenanceActionType ();
      AddMaintenanceActionStatus ();

      Database.AddTable (TableName.MAINTENANCE_ACTION,
        new Column (ColumnName.MAINTENANCE_ACTION_ID, System.Data.DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
        new Column (TableName.MAINTENANCE_ACTION + "version", DbType.Int32, ColumnProperty.NotNull, 1),
        new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
        new Column (TableName.MAINTENANCE_ACTION + "title", DbType.String, ColumnProperty.NotNull),
        new Column (TableName.MAINTENANCE_ACTION + "description", DbType.String, ColumnProperty.NotNull),
        new Column (ColumnName.MAINTENANCE_ACTION_TYPE_ID, DbType.Int32, ColumnProperty.NotNull, 1),
        new Column (ColumnName.MAINTENANCE_ACTION_STATUS_ID, DbType.Int32, ColumnProperty.NotNull, 1),
        new Column (TableName.MAINTENANCE_ACTION + "creationdatetime", DbType.DateTime, ColumnProperty.NotNull),
        new Column (TableName.MAINTENANCE_ACTION + "modifieddatetime", DbType.DateTime, ColumnProperty.NotNull),
        new Column (TableName.MAINTENANCE_ACTION + "stopdatetime", DbType.DateTime), // Date/time when the machine stopped (for curative mainly)
        new Column (TableName.MAINTENANCE_ACTION + "planneddatetime", DbType.DateTime), // Fixed scheduled date/time
        new Column (TableName.MAINTENANCE_ACTION + "remainingmachiningduration", DbType.Int32), // Remaining machining duration in seconds before the maintenance action is required
        new Column (TableName.MAINTENANCE_ACTION + "standardmachiningfrequency", DbType.Int32), // Standard machining duration
        new Column (TableName.MAINTENANCE_ACTION + "standardtotalfrequency", DbType.Int32) // Frequency in seconds
        );
      Database.ExecuteNonQuery (@"
ALTER TABLE maintenanceaction
ALTER COLUMN maintenanceactioncreationdatetime 
SET DEFAULT now() AT TIME ZONE 'UTC';");
      Database.MakeColumnText (TableName.MAINTENANCE_ACTION, TableName.MAINTENANCE_ACTION + "description");
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION, ColumnName.MACHINE_ID,
        TableName.MACHINE, ColumnName.MACHINE_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION, ColumnName.MAINTENANCE_ACTION_TYPE_ID,
        TableName.MAINTENANCE_ACTION_TYPE, ColumnName.MAINTENANCE_ACTION_TYPE_ID,
        Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION, ColumnName.MAINTENANCE_ACTION_STATUS_ID,
        TableName.MAINTENANCE_ACTION_STATUS, ColumnName.MAINTENANCE_ACTION_STATUS_ID,
        Migrator.Framework.ForeignKeyConstraint.Restrict);

      Database.AddNamedIndexCondition ("maintenanceaction_notcompleted_idx", TableName.MAINTENANCE_ACTION,
        "maintenanceactionstatusid <> 2",
        ColumnName.MACHINE_ID, TableName.MAINTENANCE_ACTION + "creationdatetime");
      Database.AddIndex (TableName.MAINTENANCE_ACTION, ColumnName.MACHINE_ID, ColumnName.MAINTENANCE_ACTION_STATUS_ID,
        ColumnName.MAINTENANCE_ACTION_TYPE_ID, TableName.MAINTENANCE_ACTION + "creationdatetime");
    }

    void AddMaintenanceActionUpdate ()
    {
      Database.AddTable (TableName.MAINTENANCE_ACTION_UPDATE,
        new Column (ColumnName.MAINTENANCE_ACTION_UPDATE_ID, System.Data.DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
        new Column (TableName.MAINTENANCE_ACTION_UPDATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
        new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
        new Column (ColumnName.MAINTENANCE_ACTION_ID, DbType.Int32, ColumnProperty.NotNull),
        new Column (TableName.UPDATER + "id", DbType.Int32),
        new Column (TableName.MAINTENANCE_ACTION_UPDATE + "comment", DbType.String),
        new Column (TableName.MAINTENANCE_ACTION_UPDATE + "oldstatusid", DbType.Int32, ColumnProperty.NotNull),
        new Column (TableName.MAINTENANCE_ACTION_UPDATE + "newstatusid", DbType.Int32, ColumnProperty.NotNull),
        new Column (TableName.MAINTENANCE_ACTION_UPDATE + "datetime", DbType.DateTime, ColumnProperty.NotNull)
        );
      Database.ExecuteNonQuery (@"
ALTER TABLE maintenanceactionupdate
ALTER COLUMN maintenanceactionupdatedatetime 
SET DEFAULT now() AT TIME ZONE 'UTC';");
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION_UPDATE, ColumnName.MACHINE_ID,
        TableName.MACHINE, ColumnName.MACHINE_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION_UPDATE, ColumnName.MAINTENANCE_ACTION_ID,
        TableName.MAINTENANCE_ACTION, ColumnName.MAINTENANCE_ACTION_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION_UPDATE, TableName.MAINTENANCE_ACTION_UPDATE + "oldstatusid",
        TableName.MAINTENANCE_ACTION_STATUS, ColumnName.MAINTENANCE_ACTION_STATUS_ID,
        Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION_UPDATE, TableName.MAINTENANCE_ACTION_UPDATE + "newstatusid",
        TableName.MAINTENANCE_ACTION_STATUS, ColumnName.MAINTENANCE_ACTION_STATUS_ID,
        Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.MAINTENANCE_ACTION_UPDATE, TableName.UPDATER + "id",
        TableName.UPDATER, TableName.UPDATER + "id",
        Migrator.Framework.ForeignKeyConstraint.SetNull);

      Database.AddIndex (TableName.MAINTENANCE_ACTION_UPDATE, ColumnName.MAINTENANCE_ACTION_ID);
    }

    void Install2 ()
    {
      Database.AddColumn (TableName.MAINTENANCE_ACTION,
        new Column (TableName.MAINTENANCE_ACTION + "completiondatetime", DbType.DateTime));
      Database.AddIndex (TableName.MAINTENANCE_ACTION, ColumnName.MACHINE_ID, TableName.MAINTENANCE_ACTION + "completiondatetime");
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      Database.RemoveTable (TableName.MAINTENANCE_ACTION_UPDATE);
      Database.RemoveTable (TableName.MAINTENANCE_ACTION);
      Database.RemoveTable (TableName.MAINTENANCE_ACTION_TYPE);
      Database.RemoveTable (TableName.MAINTENANCE_ACTION_STATUS);
    }
    #endregion // Migration
    #endregion // Methods
  }
}
