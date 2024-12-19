// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using System.Threading.Tasks;
using Lemoine.Extensions.Interfaces;
using System.Threading;
using Lemoine.Extensions.ExtensionsProvider;

namespace Lemoine.Extensions
{
  /// <summary>
  /// This class scans all dlls within a directory and retrieve the extensions from them
  /// Extensions are kept only if they are registered in the database.
  /// They are configured according to the database.
  /// 
  /// Threadsafe
  /// </summary>
  public class ExtensionManager
  {
    #region Members
    readonly AdditionalExtensionsOnlyProvider m_additionalExtensionsOnlyProvider = new AdditionalExtensionsOnlyProvider ();
    IExtensionsProvider m_setExtensionsProvider = null;
    IExtensionsProvider m_extensionsProvider = null;
    bool m_additionalExtensionsActive = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ExtensionManager).FullName);
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    #region Getters / Setters
    /// <summary>
    /// Have the extensions been loaded ?
    /// </summary>
    public static bool Loaded => ExtensionsProvider.Loaded;

    /// <summary>
    /// Plugins in error
    /// </summary>
    public static IEnumerable<IPluginDllLoader> LoadErrorPlugins => Instance.m_extensionsProvider.LoadErrorPlugins;

    /// <summary>
    /// Associated IExtensionsProvider
    /// </summary>
    public static IExtensionsProvider ExtensionsProvider
    {
      get {
        if (Instance.m_extensionsProvider is null) {
          log.Warn ("ExtensionsProvider.get: not defined, call Initialize first");
          return Instance.m_additionalExtensionsOnlyProvider;
        }
        return Instance.m_extensionsProvider;
      }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Private constructor (singleton class!)
    /// </summary>
    private ExtensionManager ()
    {
    }

    #region Methods
    /// <summary>
    /// Initialize the extension manager
    /// </summary>
    /// <param name="extensionsProvider"></param>
    /// <param name="force"></param>
    public static void Initialize (IExtensionsProvider extensionsProvider, bool force = false)
    {
      if (Instance.m_extensionsProvider is null) {
        Instance.m_setExtensionsProvider = extensionsProvider;
        if (Instance.m_additionalExtensionsActive) {
          Instance.m_extensionsProvider = new MultiExtensionsProvider (extensionsProvider, Instance.m_additionalExtensionsOnlyProvider);
        }
        else {
          Instance.m_extensionsProvider = extensionsProvider;
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("Initialize: already initialized");
        }
        if (Instance.m_setExtensionsProvider != extensionsProvider) {
          if (log.IsWarnEnabled) {
            log.Warn ($"Initialize: try to set a new extensionsProvider, force={force}");
          }
          if (force) {
            Instance.m_additionalExtensionsOnlyProvider?.Clear ();
            Instance.m_additionalExtensionsActive = false;
            Instance.m_setExtensionsProvider = extensionsProvider;
            Instance.m_extensionsProvider = extensionsProvider;
          }
        }
      }
    }

    /// <summary>
    /// Are the extensions active ?
    /// </summary>
    /// <returns></returns>
    public static bool IsActive ()
    {
      return ExtensionsProvider.IsActive ();
    }

    /// <summary>
    /// Activate only the NHibernateExtensions
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    public static void ActivateNHibernateExtensions (bool pluginUserDirectoryActive)
    {
      ExtensionsProvider.ActivateNHibernateExtensions (pluginUserDirectoryActive);
    }

    /// <summary>
    /// Activate the extensions
    /// </summary>
    /// <param name="pluginUserDirectoryActive">use the synchronized user directory</param>
    public static void Activate (bool pluginUserDirectoryActive = true)
    {
      ExtensionsProvider.Activate (pluginUserDirectoryActive);
    }

    /// <summary>
    /// De-activate the extensions
    /// </summary>
    public static void Deactivate ()
    {
      ExtensionsProvider.Deactivate ();
    }

    /// <summary>
    /// Clear the plugins in cache and de-activate the extensions
    /// </summary>
    public static void ClearDeactivate ()
    {
      ExtensionsProvider.ClearDeactivate ();
    }

    /// <summary>
    /// Clear all the additional extensions (used for the tests for example)
    /// </summary>
    public static void ClearAdditionalExtensions ()
    {
      Instance.m_additionalExtensionsOnlyProvider.Clear ();
    }

    /// <summary>
    /// Add an extension manually, for example for the tests or for specific applications
    /// </summary>
    /// <param name="extensionType"></param>
    public static void Add (Type extensionType)
    {
      Instance.m_additionalExtensionsOnlyProvider.Add (extensionType);
      if (!Instance.m_additionalExtensionsActive && (null != Instance.m_extensionsProvider)) {
        Instance.m_extensionsProvider = new MultiExtensionsProvider (Instance.m_setExtensionsProvider, Instance.m_additionalExtensionsOnlyProvider);
      }
      Instance.m_additionalExtensionsActive = true;
    }

    /// <summary>
    /// Add an extension manually, for example for the tests or for the specific applications
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void Add<T> ()
    {
      Add (typeof (T));
    }

    /// <summary>
    /// Create new extensions that follow the specific interface
    /// </summary>
    /// <param name="packageIdentifier">If set, restrict the extensions to this package identifier</param>
    /// <param name="checkedThread">checked thread (nullable)</param>
    /// <returns></returns>
    public static IEnumerable<T> GetExtensions<T> (string packageIdentifier = "", Lemoine.Threading.IChecked checkedThread = null)
      where T : IExtension
    {
      return ExtensionsProvider.GetExtensions<T> (packageIdentifier: packageIdentifier, checkedThread: checkedThread);
    }

    /// <summary>
    /// Get the NHibernate extensions (before the database connection is completed)
    /// 
    /// It may returns also plugins that are not active / valid
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <returns></returns>
    public static IEnumerable<T> LoadAndGetNHibernateExtensions<T> (Lemoine.Threading.IChecked checkedThread = null)
      where T : IExtension
    {
      return ExtensionsProvider.LoadAndGetNHibernateExtensions<T> (checkedThread: checkedThread);
    }

    /// <summary>
    /// Reload the plugins with the specified plugin filter
    /// </summary>
    /// <param name="pluginFilter"></param>
    public static void Reload (IPluginFilter pluginFilter)
    {
      ExtensionsProvider.Reload (pluginFilter);
    }

    /// <summary>
    /// Reload the plugins
    /// </summary>
    /// <param name="checkedThread"></param>
    public static void Reload (Lemoine.Threading.IChecked checkedThread = null)
    {
      ExtensionsProvider.Reload (checkedThread);
    }

    /// <summary>
    /// Load the extensions if they have not been loaded before yet
    /// 
    /// This is safer to call that first not inside a transaction,
    /// else there may be an unexpected Rollback in case of a failed upgrade for example
    /// </summary>
    /// <param name="checkedThread"></param>
    public static void Load (Lemoine.Threading.IChecked checkedThread = null)
    {
      ExtensionsProvider.Load (checkedThread);
    }

    /// <summary>
    /// Load the extensions if they have not been loaded before yet
    /// 
    /// This is safer to call that first not inside a transaction,
    /// else there may be an unexpected Rollback in case of a failed upgrade for example
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="checkedThread"></param>
    public static async Task LoadAsync (CancellationToken cancellationToken, Lemoine.Threading.IChecked checkedThread = null)
    {
      await ExtensionsProvider.LoadAsync (cancellationToken, checkedThread);
    }

    /// <summary>
    /// Return the plugin identified by a name
    /// 
    /// If not found, return null
    /// </summary>
    /// <param name="identifyingName">not null and not empty</param>
    /// <returns></returns>
    static public IPluginDll GetPlugin (string identifyingName)
    {
      return ExtensionsProvider.GetPlugin (identifyingName);
    }
    #endregion // Methods

    #region Instance
    static ExtensionManager Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
      static Nested () { }
      internal static readonly ExtensionManager instance = new ExtensionManager ();
    }
    #endregion // Instance
  }
}
