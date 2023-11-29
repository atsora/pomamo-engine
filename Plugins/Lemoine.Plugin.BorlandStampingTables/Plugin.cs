// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.GDBMigration;
using Lemoine.Model;
using Pulse.Extensions.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.Plugin.BorlandStampingTables
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Members
    TransformationProviderExt m_database = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "BorlandStampingTables"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get {
        return "Create the database tables that are used by the old Borland stamping program if they don't exist (deprecated)";
      }
    }

    public PluginFlag Flags
    {
      get {
        return PluginFlag.Config;
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 2;

    TransformationProviderExt Database
    {
      get {
        if (null == m_database) {
          m_database = new TransformationProviderExt ();
        }
        return m_database;
      }
    }
    #endregion // Getters / Setters

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
        break;
      case 2:
        // Do not the remove tables yet (later....)
        break;
      default:
        throw new InvalidOperationException ();
      }
    }

    #region Migration
    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      if (Database.TableExists (TableName.SFK_CAMSYSTEM)) {
        Database.RemoveTable (TableName.SFK_CAMSYSTEM);
      }
      if (Database.TableExists (TableName.SFK_MACHTYPE)) {
        Database.RemoveTable (TableName.SFK_MACHTYPE);
      }
    }
    #endregion // Migration

    #endregion // Methods
  }
}
