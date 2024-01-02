// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Plugins loader
  /// </summary>
  public interface IPluginsLoader
  {
    /// <summary>
    /// Are the plugins loaded
    /// </summary>
    bool Loaded { get; }

    /// <summary>
    /// Load the plugins
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    void Load (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread);

    /// <summary>
    /// Load the plugins the plugins asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    Task LoadAsync (CancellationToken cancellationToken, bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread);

    /// <summary>
    /// Reload the plugins
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    void Reload (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread);

    /// <summary>
    /// Clear the loaded plugins
    /// </summary>
    /// <param name="checkedThread"></param>
    void Clear (Lemoine.Threading.IChecked checkedThread);

    /// <summary>
    /// Return the plugins that were in error during the load
    /// 
    /// No automatic load here, it must be loaded first
    /// </summary>
    /// <returns></returns>
    IEnumerable<IPluginDllLoader> GetLoadErrorPlugins ();

    /// <summary>
    /// Get the active plugins
    /// 
    /// The plugins needs to be loaded before. See <see cref="IExtensionsLoader"/>
    /// </summary>
    IList<IPluginDll> GetActivePlugins ();
  }

  /// <summary>
  /// Interface for a loader of NHibernate plugins
  /// </summary>
  public interface INHibernatePluginsLoader : IPluginsLoader
  {
  }

  /// <summary>
  /// Interface for the main loader of plugins
  /// </summary>
  public interface IMainPluginsLoader : IPluginsLoader
  { 
  }
}
