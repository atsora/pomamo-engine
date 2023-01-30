// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Plugin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Class that provides extensions
  /// </summary>
  public interface IExtensionsProvider
  {
    /// <summary>
    /// Have the extensions been loaded ?
    /// </summary>
    bool Loaded { get; }

    /// <summary>
    /// Plugins in error
    /// </summary>
    IEnumerable<PluginDllLoader> LoadErrorPlugins { get; }

    /// <summary>
    /// Are the extensions active ?
    /// </summary>
    /// <returns></returns>
    bool IsActive ();

    /// <summary>
    /// Activate only the NHibernateExtensions
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    void ActivateNHibernateExtensions (bool pluginUserDirectoryActive);

    /// <summary>
    /// Activate the extensions
    /// </summary>
    /// <param name="pluginUserDirectoryActive">use the synchronized user directory</param>
    void Activate (bool pluginUserDirectoryActive = true);

    /// <summary>
    /// De-activate the extensions
    /// </summary>
    void Deactivate ();

    /// <summary>
    /// Clear the plugins in cache and de-activate the extensions
    /// </summary>
    void ClearDeactivate ();

    /// <summary>
    /// Create new extensions that follow the specific interface
    /// 
    /// The plugins needs to be loaded before. See <see cref="IExtensionsLoader"/>
    /// </summary>
    /// <param name="packageIdentifier">If set, restrict the extensions to this package identifier</param>
    /// <param name="checkedThread">checked thread (nullable)</param>
    /// <returns></returns>
    IEnumerable<T> GetExtensions<T> (string packageIdentifier = "", Lemoine.Threading.IChecked checkedThread = null) where T : IExtension;

    /// <summary>
    /// Get the NHibernate extensions (before the database connection is completed)
    /// 
    /// It may returns also plugins that are not active / valid
    /// 
    /// The plugins needs to be loaded before. See <see cref="IExtensionsLoader"/>
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
    IEnumerable<T> LoadAndGetNHibernateExtensions<T> (Lemoine.Threading.IChecked checkedThread = null)
      where T : IExtension;

    /// <summary>
    /// Reload the plugins
    /// </summary>
    /// <param name="checkedThread"></param>
    void Reload (Lemoine.Threading.IChecked checkedThread = null);

    /// <summary>
    /// Reload the plugins with the specified plugin filter
    /// </summary>
    /// <param name="pluginFilter"></param>
    void Reload (IPluginFilter pluginFilter);

    /// <summary>
    /// Load the extensions if they have not been loaded before yet
    /// 
    /// This is safer to call that first not inside a transaction,
    /// else there may be an unexpected Rollback in case of a failed upgrade for example
    /// </summary>
    /// <param name="checkedThread"></param>
    void Load (Lemoine.Threading.IChecked checkedThread = null);

    /// <summary>
    /// Load the extensions if they have not been loaded before yet
    /// 
    /// This is safer to call that first not inside a transaction,
    /// else there may be an unexpected Rollback in case of a failed upgrade for example
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="checkedThread"></param>
    Task LoadAsync (CancellationToken cancellationToken, Lemoine.Threading.IChecked checkedThread = null);

    /// <summary>
    /// Return the plugin identified by a name
    /// 
    /// If not found, return null
    /// </summary>
    /// <param name="identifyingName">not null and not empty</param>
    /// <returns></returns>
    IPluginDll GetPlugin (string identifyingName);
  }
}
