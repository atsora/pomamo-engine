// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.FileRepository;
using Lemoine.Info;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using System.Threading.Tasks;
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Plugin;
using System.Reflection;
using System.Threading;
using Lemoine.Conversion;

namespace Lemoine.Extensions.ExtensionsProvider
{
  /// <summary>
  /// Default class to provides extensions
  /// 
  /// Threadsafe
  /// </summary>
  public class ExtensionsProvider : IExtensionsProvider
  {
    static readonly string CHECK_INTERFACES_KEY = "Extensions.CheckInterfaces";
    static readonly bool CHECK_INTERFACES_DEFAULT = false;

    #region Members
    readonly IDatabaseConnectionStatus m_databaseConnectionStatus;
    readonly IPluginFilter m_pluginFilter;
    readonly IPluginsLoader m_pluginsLoader;
    readonly IPluginsLoader m_nhibernatePluginsLoader;

    volatile bool m_nhibernateExtensionsActive = false;
    volatile bool m_nhibernateExtensionsPluginUserDirectoryActive = false;

    volatile bool m_active = false;
    volatile bool m_pluginUserDirectoryActive = false;
    volatile bool m_loaded = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ExtensionsProvider).FullName);
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    #region Getters / Setters
    /// <summary>
    /// Have the extensions been loaded ?
    /// </summary>
    public bool Loaded => m_loaded;

    /// <summary>
    /// Plugin flag filter
    /// </summary>
    public IPluginFilter PluginFilter => m_pluginFilter;

    /// <summary>
    /// Plugins in error
    /// </summary>
    public IEnumerable<IPluginDllLoader> LoadErrorPlugins
    {
      get {
        Load ();
        return m_pluginsLoader.GetLoadErrorPlugins ();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="databaseConnectionStatus"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="interfaceProviders">not null</param>
    /// <param name="pluginsLoader">not null</param>
    /// <param name="nhibernatePluginsLoader">not null</param>
    public ExtensionsProvider (IDatabaseConnectionStatus databaseConnectionStatus, IPluginFilter pluginFilter, IEnumerable<IExtensionInterfaceProvider> interfaceProviders, IMainPluginsLoader pluginsLoader, INHibernatePluginsLoader nhibernatePluginsLoader)
    {
      Debug.Assert (null != databaseConnectionStatus);
      Debug.Assert (null != interfaceProviders);
      Debug.Assert (null != pluginsLoader);
      Debug.Assert (null != nhibernatePluginsLoader);

      if (databaseConnectionStatus is null) {
        log.Fatal ("ExtensionsProvider: database connection status is null");
      }

      m_databaseConnectionStatus = databaseConnectionStatus;
      m_pluginFilter = pluginFilter;
      m_pluginsLoader = pluginsLoader;
      m_nhibernatePluginsLoader = nhibernatePluginsLoader;

      foreach (var interfaceProvider in interfaceProviders) {
        interfaceProvider.Load ();
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Are the extensions active ?
    /// </summary>
    /// <returns></returns>
    public bool IsActive ()
    {
      return m_active;
    }

    /// <summary>
    /// Activate only the NHibernateExtensions
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    public void ActivateNHibernateExtensions (bool pluginUserDirectoryActive)
    {
      m_nhibernateExtensionsPluginUserDirectoryActive = pluginUserDirectoryActive;
      m_nhibernateExtensionsActive = true;
    }

    /// <summary>
    /// Activate the extensions
    /// </summary>
    /// <param name="pluginUserDirectoryActive">use the synchronized user directory</param>
    public void Activate (bool pluginUserDirectoryActive = true)
    {
      m_active = true;
      m_pluginUserDirectoryActive = pluginUserDirectoryActive;
    }

    /// <summary>
    /// De-activate the extensions
    /// </summary>
    public void Deactivate ()
    {
      m_active = false;
    }

    /// <summary>
    /// Clear the plugins in cache and de-activate the extensions
    /// </summary>
    public void ClearDeactivate ()
    {
      m_pluginsLoader.Clear (null);
      m_loaded = false;
      Deactivate ();
    }

    /// <summary>
    /// Create new extensions that follow the specific interface
    /// </summary>
    /// <param name="packageIdentifier">If defined, restrict the extensions to this package identifier</param>
    /// <param name="checkedThread">checked thread (nullable)</param>
    /// <returns></returns>
    public IEnumerable<T> GetExtensions<T> (string packageIdentifier = "", Lemoine.Threading.IChecked checkedThread = null)
      where T : IExtension
    {
      IList<T> extensions = new List<T> ();

      if (!m_databaseConnectionStatus.IsDatabaseConnectionUp) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetExtensions: do not return any extension because the database connection has not been started yet");
        }
        return extensions;
      }

      if (!m_active) {
        log.Debug ("GetExtensions: extensions are not active => return an empty list or only the additional extensions");
        return extensions;
      }

      if (!m_pluginsLoader.Loaded) {
        log.Warn ("GetExtensions: extensions were not loaded. Please load them first with the IExtensionsLoader");
        return extensions;
      }

      checkedThread?.SetActive ();

      var plugins = m_pluginsLoader.GetActivePlugins ();
      foreach (var plugin in plugins.Distinct ()) {
        checkedThread?.SetActive ();
        var activeExtensionTypes = plugin.ActiveExtensionTypes
          .Where (type => IsValidInterfaceImplementation<T> (type));
        // Note: in .Net Core, replace it by type.GetTypeInfo ().IsAbstract
        foreach (var activeExtensionType in activeExtensionTypes) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetExtensions: add extension of type {activeExtensionType} for plugin {plugin}");
          }
          checkedThread?.SetActive ();
          bool uniqueInstance = false;
          IExtension extension = null;
          var instances = plugin.Instances
            .Where (a => a.InstanceActive);
          if (!string.IsNullOrEmpty (packageIdentifier)) {
            log.Info ($"GetExtensions: restrict the extensions to package identifier {packageIdentifier}");
            instances = instances
              .Where (a => a.PackageIdentifyingName.Equals (packageIdentifier));
          }
          foreach (var instance in instances) {
            try {
              if (!uniqueInstance) {
                extension = (IExtension)Activator.CreateInstance (activeExtensionType);
                uniqueInstance = extension.UniqueInstance;
                if (!(extension is T convertedExtension)) {
                  log.Error ($"GetExtensions: {extension} not directly a {typeof (T)}, this should not happen but try to use an auto-converter");
                  var autoConverter = new DefaultAutoConverter ();
                  convertedExtension = autoConverter.ConvertAuto<T> (extension);
                }
                extensions.Add (convertedExtension);
              }
              Debug.Assert (null != extension);
              if (extension is Lemoine.Extensions.Extension.IConfigurable configurableExtension) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetExtensions: {extension} is a configurable extension, attach the instance {instance}");
                }
                configurableExtension.AddConfigurationContext (instance);
              }
            }
            catch (Exception ex) {
              log.Error ($"GetExtensions: could not create extension of type {activeExtensionType}", ex);
            }
          }
        }
      }

      return extensions;
    }

    bool IsValidInterfaceImplementation<T> (Type type)
    {
      var extensionInterface = typeof (T).GetTypeInfo ();
      if (type.GetTypeInfo ().IsAbstract) {
        if (log.IsTraceEnabled) {
          log.Trace ($"IsValidInterfaceImplementation: {type} is abstract => return false");
        }
        return false;
      }
      var result = extensionInterface.IsAssignableFrom (type);
      if (log.IsTraceEnabled) {
        if (result) {
          log.Trace ($"IsValidInterfaceImplementation: {type} implements {typeof (T)}");
        }
        else {
          log.Trace ($"IsValidInterfaceImplementation: {type} does not implement {typeof (T)}");
        }
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (CHECK_INTERFACES_KEY, CHECK_INTERFACES_DEFAULT)) {
        if (!result) {
          var interfaces = type.GetInterfaces ();
          foreach (var typeInterface in interfaces) {
            if (typeInterface.FullName.Equals (extensionInterface.FullName)) {
              log.Error ($"IsValidInterfaceImplementation: {type} implements {typeInterface.FullName} but not {extensionInterface}");
              return true;
            }
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Get the NHibernate extensions (before the database connection is completed)
    /// 
    /// It may returns also plugins that are not active / valid
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
    public IEnumerable<T> LoadAndGetNHibernateExtensions<T> (Lemoine.Threading.IChecked checkedThread = null)
      where T : IExtension
    {
      IList<T> extensions = new List<T> ();

      if (!m_nhibernateExtensionsActive) {
        log.Debug ("GetNHibernateExtensions: NHibernate extensions are not active => return an empty list or only the additional extensions");
        return extensions;
      }

      pluginStatusLog.DebugFormat ("Extensions of type {0} requested without the database connection",
        typeof (T));

      var plugins = GetAllNHibernateExtensionPlugins (checkedThread);
      foreach (var plugin in plugins.Distinct ()) {
        var activeExtensionTypes = plugin.ActiveExtensionTypes
          .Where (type => typeof (T).IsAssignableFrom (type));
        foreach (var activeExtensionType in activeExtensionTypes) {
          log.DebugFormat ("GetNHibernateExtensions: " +
                           "add extension of type {0} for plugin {1}",
                           activeExtensionType, plugin);
          var extension = Activator.CreateInstance (activeExtensionType);
          extensions.Add ((T)extension);
        }
      }

      return extensions;
    }

    /// <summary>
    /// Reload the plugins with the specified plugin flag filter
    /// </summary>
    /// <param name="pluginFilter"></param>
    public void Reload (IPluginFilter pluginFilter)
    {
      m_pluginsLoader.Reload (m_pluginUserDirectoryActive, pluginFilter, null);
    }

    /// <summary>
    /// Reload the plugins
    /// </summary>
    /// <param name="checkedThread"></param>
    public void Reload (Lemoine.Threading.IChecked checkedThread = null)
    {
      m_pluginsLoader.Reload (m_pluginUserDirectoryActive, m_pluginFilter, checkedThread);
    }

    /// <summary>
    /// Load the extensions if they have not been loaded before yet
    /// 
    /// This is safer to call that first not inside a transaction,
    /// else there may be an unexpected Rollback in case of a failed upgrade for example
    /// </summary>
    /// <param name="checkedThread"></param>
    public void Load (Lemoine.Threading.IChecked checkedThread = null)
    {
      m_pluginsLoader.Load (m_pluginUserDirectoryActive, m_pluginFilter, checkedThread);
      m_loaded = true;
    }

    /// <summary>
    /// Load the extensions if they have not been loaded before yet asynchronously
    /// 
    /// This is safer to call that first not inside a transaction,
    /// else there may be an unexpected Rollback in case of a failed upgrade for example
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="checkedThread"></param>
    public async Task LoadAsync (CancellationToken cancellationToken, Lemoine.Threading.IChecked checkedThread = null)
    {
      await m_pluginsLoader.LoadAsync (cancellationToken, m_pluginUserDirectoryActive, m_pluginFilter, checkedThread);
      m_loaded = true;
    }

    /// <summary>
    /// Get the all plugins without any database connection
    /// 
    /// Plugins that are be active / valid may be returned here
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
    IEnumerable<IPluginDll> GetAllNHibernateExtensionPlugins (Lemoine.Threading.IChecked checkedThread = null)
    {
      bool samePluginUserDirectoryActiveOption =
        m_nhibernateExtensionsPluginUserDirectoryActive == m_pluginUserDirectoryActive;
      if (m_pluginsLoader.Loaded && samePluginUserDirectoryActiveOption) {
        pluginStatusLog.DebugFormat ("The plugins were already loaded with the database connection, return them");
        return m_pluginsLoader.GetActivePlugins ();
      }

      m_nhibernatePluginsLoader.Load (m_nhibernateExtensionsPluginUserDirectoryActive, m_pluginFilter, checkedThread);
      return m_nhibernatePluginsLoader.GetActivePlugins ();
    }

    /// <summary>
    /// Return the plugin identified by a name
    /// 
    /// If not found, return null
    /// </summary>
    /// <param name="identifyingName">not null and not empty</param>
    /// <returns></returns>
    public IPluginDll GetPlugin (string identifyingName)
    {
      Debug.Assert (null != identifyingName);
      Debug.Assert (!string.IsNullOrEmpty (identifyingName));

      return m_pluginsLoader.GetActivePlugins ()
          .FirstOrDefault (plugin => identifyingName.Equals (plugin.IdentifyingName));
    }
    #endregion // Methods
  }
}
