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
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Extensions.Plugin;
using Lemoine.ModelDAO;
using System.Threading.Tasks;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Plugin;
using System.Threading;
using Pulse.Extensions.Plugin;

namespace Pulse.Extensions
{
  /// <summary>
  /// Class to synchronize and load the plugin directories only once (eager load)
  /// 
  /// Threadsafe
  /// </summary>
  public class PluginsLoader : IPluginsLoader, IMainPluginsLoader
  {
    #region Members
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IPluginDirectories m_pluginDirectories;

    bool? m_pluginUserDirectoryActive = null;
    PluginFlag? m_pluginFlagFilter = null;
    int m_loadStatus = 0; // 1: in progress, 2: done
    int m_loadingThreadId = 0;

    readonly IList<IPluginDll> m_activePlugins = new List<IPluginDll> ();
    readonly IList<PluginDllLoader> m_loadErrorPlugins = new List<PluginDllLoader> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PluginsLoader).FullName);
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    #region Getters / Setters
    /// <summary>
    /// Are the plugins loaded
    /// </summary>
    public bool Loaded
    {
      get { return m_loadStatus == 2; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="assemblyLoader">not null</param>
    /// <param name="pluginSynchronizationTimeoutProvider">optional</param>
    public PluginsLoader (IAssemblyLoader assemblyLoader, IPluginSynchronizationTimeoutProvider pluginSynchronizationTimeoutProvider = null)
    {
      Debug.Assert (null != assemblyLoader);

      m_assemblyLoader = assemblyLoader;
      var pluginDirectories = new Lemoine.Extensions.Plugin.PluginDirectories.PluginDirectories (assemblyLoader);
      if (null != pluginSynchronizationTimeoutProvider) {
        pluginDirectories.SynchronizationTimeout = pluginSynchronizationTimeoutProvider.PluginSynchronizationTimeout;
      };
      m_pluginDirectories = pluginDirectories;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Clear the loaded plugins
    /// </summary>
    /// <param name="checkedThread"></param>
    public void Clear (Lemoine.Threading.IChecked checkedThread)
    {
      var loadStatus = System.Threading.Interlocked.CompareExchange (ref m_loadStatus, 1, 2);
      switch (loadStatus) {
      case 0: // Not done: nothing to do
        break;
      case 1: // In progress
      {
        var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        if (threadId == m_loadingThreadId) {
          log.ErrorFormat ("Clear: current loading thread => unexpected");
          throw new Exception ("Clear/Load from the same thread");
        }
        else {
          log.DebugFormat ("Clear: waiting for completion");
          while (1 == m_loadStatus) { // Still in progress
            if (null != checkedThread) {
              checkedThread.SetActive ();
            }
            System.Threading.Thread.Sleep (100);
          }
          Clear (checkedThread);
          return;
        }
      }
      case 2: // Completed
        var currentLoadingThreadId = System.Threading.Interlocked.CompareExchange (ref m_loadingThreadId, System.Threading.Thread.CurrentThread.ManagedThreadId, 0);
        if (0 != currentLoadingThreadId) {
          log.Fatal ($"Clear: already started an another thread, unexpected, at {System.Environment.StackTrace}");
          throw new InvalidOperationException ("Clear can be run");
        }
        try {
          pluginStatusLog.Info ("Clear of the plugins requested");
          if (pluginStatusLog.IsDebugEnabled) {
            pluginStatusLog.Debug ($"Clear of the plugins requested by {System.Environment.StackTrace}");
          }
          if (null != checkedThread) {
            checkedThread.SetActive ();
          }
          m_activePlugins.Clear ();
          m_loadErrorPlugins.Clear ();
        }
        catch (Exception ex) {
          log.Error ("Clear: exception", ex);
          throw;
        }
        finally {
          System.Threading.Interlocked.Exchange (ref m_loadingThreadId, 0);
          System.Threading.Interlocked.Exchange (ref m_loadStatus, 0); // Switch to 0 (Not done)
        }
        break;
      default:
        log.Fatal ($"Clear: unexpected status {loadStatus}");
        throw new InvalidOperationException ();
      }
    }

    void CheckCache (bool pluginUserDirectoryActive, IPluginFilter pluginFilter)
    {
      if (m_pluginUserDirectoryActive.HasValue
        && (pluginUserDirectoryActive != m_pluginUserDirectoryActive.Value)) { // New pluginUserDirectoryActive parameter
        log.Warn ("CheckCache: pluginUserDirectoryActive parameter changed, reset the cache");
        m_loadStatus = 0;
      }
      m_pluginUserDirectoryActive = pluginUserDirectoryActive;

      var pluginFlagFilter = pluginFilter.GetPluginFlag ();
      if (m_pluginFlagFilter.Equals (pluginFlagFilter)) {
        log.WarnFormat ("CheckCache: pluginFlagFilter parameter changed, reset the cache");
        m_loadStatus = 0;
      }
      m_pluginFlagFilter = pluginFlagFilter;
    }

    /// <summary>
    /// Load the plugins
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    public void Load (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
    {
      CheckCache (pluginUserDirectoryActive, pluginFilter);
      InstanceLoad (pluginUserDirectoryActive, pluginFilter, checkedThread);
    }

    /// <summary>
    /// Load the plugins
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    public async Task LoadAsync (CancellationToken cancellationToken, bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
    {
      CheckCache (pluginUserDirectoryActive, pluginFilter);
      await InstanceLoadAsync (cancellationToken, pluginUserDirectoryActive, pluginFilter, checkedThread);
    }

    /// <summary>
    /// Reload the plugins
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginFilter"></param>
    /// <param name="checkedThread"></param>
    public void Reload (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
    {
      pluginStatusLog.Info ("Reload of the plugins requested");
      pluginStatusLog.Debug ($"Reload of the plugins requested by {System.Environment.StackTrace}");
      m_loadStatus = 0;
      Load (pluginUserDirectoryActive, pluginFilter, checkedThread);
    }

    /// <summary>
    /// Get the active plugins
    /// </summary>
    public IList<IPluginDll> GetActivePlugins ()
    {
      if (2 != m_loadStatus) {
        log.Warn ("GetActivePluginsAsync: the plugins were not loaded yet");
      }
      return m_activePlugins;
    }

    /// <summary>
    /// Return the plugins that were in error during the load
    /// 
    /// No automatic load here, it must be loaded first
    /// </summary>
    /// <returns></returns>
    public IEnumerable<PluginDllLoader> GetLoadErrorPlugins ()
    {
      return m_loadErrorPlugins;
    }

    void InstanceLoad (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
    {
      if (null != checkedThread) {
        checkedThread.SetActive ();
      }

      var loadStatus = System.Threading.Interlocked.CompareExchange (ref m_loadStatus, 1, 0);
      switch (loadStatus) {
      case 0: // Not done, do it
        var currentLoadingThreadId = System.Threading.Interlocked.CompareExchange (ref m_loadingThreadId, System.Threading.Thread.CurrentThread.ManagedThreadId, 0);
        if (0 != currentLoadingThreadId) {
          log.Fatal ($"InstanceLoadAsync: already started an another thread, unexpected, at {System.Environment.StackTrace}");
          pluginStatusLog.Fatal ($"Load attempt in multiple threads");
          throw new InvalidOperationException ("InstanceLoadAsync can be run");
        }
        try {
          pluginStatusLog.Info ("Load of the plugins requested");
          if (pluginStatusLog.IsDebugEnabled) {
            pluginStatusLog.Debug ($"Load of the plugins requested by {System.Environment.StackTrace}");
          }
          if (null != checkedThread) {
            checkedThread.SetActive ();
          }
          LoadPlugins (pluginUserDirectoryActive, pluginFilter, checkedThread);
        }
        catch (Exception ex) {
          pluginStatusLog.Error ($"Load of the plugins failed", ex);
          log.Error ("InstanceLoad: exception", ex);
          throw;
        }
        finally {
          System.Threading.Interlocked.Exchange (ref m_loadingThreadId, 0);
          System.Threading.Interlocked.CompareExchange (ref m_loadStatus, 2, 1); // Switch to 2 (completed or exception)
        }
        return;
      case 1: // In progress, wait for the completion
        var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        if (threadId == m_loadingThreadId) {
          log.Debug ("InstanceLoad: current loading thread => return");
          return;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"InstanceLoad: waiting for completion, status={m_loadStatus}");
          }
          while (1 == m_loadStatus) { // Still in progress
            if (null != checkedThread) {
              checkedThread.SetActive ();
            }
            System.Threading.Thread.Sleep (100);
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"InstanceLoad: load completed, status={m_loadStatus}");
          }
          return;
        }
      case 2: // Completed
        return;
      default:
        log.Fatal ($"InstanceLoad: unexpected status {loadStatus}");
        pluginStatusLog.Fatal ($"Unexpected load status {loadStatus}");
        throw new InvalidOperationException ();
      }
    }

    async Task InstanceLoadAsync (CancellationToken cancellationToken, bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
    {
      if (null != checkedThread) {
        checkedThread.SetActive ();
      }

      var loadStatus = System.Threading.Interlocked.CompareExchange (ref m_loadStatus, 1, 0);
      switch (loadStatus) {
      case 0: // Not done, do it
        var currentLoadingThreadId = System.Threading.Interlocked.CompareExchange (ref m_loadingThreadId, System.Threading.Thread.CurrentThread.ManagedThreadId, 0);
        if (0 != currentLoadingThreadId) {
          log.Fatal ($"InstanceLoadAsync: already started an another thread, unexpected, at {System.Environment.StackTrace}");
          pluginStatusLog.Fatal ($"Load attempt in multiple threads");
          throw new InvalidOperationException ("InstanceLoadAsync can be run");
        }
        m_loadingThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        try {
          pluginStatusLog.Info ("Load of the plugins requested");
          if (pluginStatusLog.IsDebugEnabled) {
            pluginStatusLog.Debug ($"Load of the plugins requested by {System.Environment.StackTrace}");
          }
          if (null != checkedThread) {
            checkedThread.SetActive ();
          }
          await LoadPluginsAsync (cancellationToken, pluginUserDirectoryActive, pluginFilter, checkedThread);
        }
        catch (Exception ex) {
          pluginStatusLog.Error ($"Load of the plugins failed", ex);
          log.Error ("InstanceLoadAsync: exception", ex);
          throw;
        }
        finally {
          System.Threading.Interlocked.Exchange (ref m_loadingThreadId, 0);
          System.Threading.Interlocked.CompareExchange (ref m_loadStatus, 2, 1); // Switch to 2 (completed or exception)
        }
        return;
      case 1: // In progress, wait for the completion
        if (log.IsDebugEnabled) {
          log.Debug ($"InstanceLoadAsync: in progress, status={m_loadStatus} at {System.Environment.StackTrace}");
        }
        return;
      case 2: // Completed
        return;
      default:
        log.Fatal ($"InstanceLoadAsync: unexpected status {loadStatus}");
        pluginStatusLog.Fatal ($"Unexpected load status {loadStatus}");
        throw new InvalidOperationException ();
      }
    }

    bool IsFlagFilterMatch (IPlugin plugin, PluginFlag? pluginFlagFilter)
    {
      return !pluginFlagFilter.HasValue
        || !plugin.Flag.HasValue
        || plugin.Flag.Value.HasFlag (pluginFlagFilter.Value);
    }

    void LoadPlugins (bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
    {
      log.Debug ("LoadPlugins");

      // Clear existing extensions and plugins
      m_activePlugins.Clear ();
      m_loadErrorPlugins.Clear ();

      var pluginFlagFilter = pluginFilter.GetPluginFlag ();

      // Read the database and find all registered extensions
      IEnumerable<IPackagePluginAssociation> activePackagePluginAssociations;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ExtensionManager.LoadPlugins")) {
          activePackagePluginAssociations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
            .FindAllWithPackagePlugin ()
            .Where (a => a.Active)
            .Where (a => a.Package.Activated)
            .Where (a => IsFlagFilterMatch (a.Plugin, pluginFlagFilter));
        }
      }

      if (!activePackagePluginAssociations.Any ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadPlugins: no active package/plugin that matches flag filter {pluginFlagFilter}, return");
        }
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadPlugins: {activePackagePluginAssociations.Count ()} plugin/package associations were loaded");
      }

      // Load the plugin with their configuration / instance
      var pluginNames = activePackagePluginAssociations
        .Select (a => a.Plugin.IdentifyingName);
      var pluginDirectories = m_pluginDirectories.GetDirectories (pluginUserDirectoryActive, pluginNames: pluginNames, checkedThread: checkedThread)
        .Select (d => d.FullName);
      int failedPlugins = 0;
      foreach (var byPlugin in activePackagePluginAssociations.GroupBy (a => a.Plugin)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadPlugins: load plugin {byPlugin}");
        }
        try {
          LoadPlugin (byPlugin.Key, pluginDirectories, byPlugin, pluginFilter);
        }
        catch (Exception ex) {
          log.Error ($"LoadPlugins: load of plugin {byPlugin} failed", ex);
          pluginStatusLog.Error ($"Load of {byPlugin} failed", ex);
          failedPlugins++;
        }
      }

      if (log.IsWarnEnabled) {
        if (0 == failedPlugins) {
          log.Debug ("LoadPlugins: all the plugins were successfully loaded");
        }
        else {
          log.Warn ($"LoadPlugins: {failedPlugins} plugins failed to load");
        }
      }
      if (pluginStatusLog.IsWarnEnabled && (0 != failedPlugins)) {
        pluginStatusLog.Warn ($"{failedPlugins} plugins were not loaded");
      }
    }

    async Task LoadPluginsAsync (CancellationToken cancellationToken, bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
    {
      log.Debug ("LoadPluginsAsync");

      // Clear existing extensions and plugins
      m_activePlugins.Clear ();
      m_loadErrorPlugins.Clear ();

      var pluginFlagFilter = pluginFilter.GetPluginFlag ();

      // Read the database and find all registered extensions
      IEnumerable<IPackagePluginAssociation> activePackagePluginAssociations;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ExtensionManager.LoadPlugins")) {
          activePackagePluginAssociations = await ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
            .FindAllWithPackagePluginAsync ();
          activePackagePluginAssociations = activePackagePluginAssociations
            .Where (a => a.Active)
            .Where (a => a.Package.Activated)
            .Where (a => IsFlagFilterMatch (a.Plugin, pluginFlagFilter));
        }
      }

      if (!activePackagePluginAssociations.Any ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadPluginsAsync: no active package/plugin that matches flag filter {pluginFlagFilter}, return");
        }
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadPluginsAsync: {activePackagePluginAssociations.Count ()} plugin/package associations were loaded");
      }

      // Load the plugin with their configuration / instance
      var pluginNames = activePackagePluginAssociations
        .Select (a => a.Plugin.IdentifyingName);
      var pluginDirectories = m_pluginDirectories.GetDirectories (pluginUserDirectoryActive, pluginNames: pluginNames, checkedThread: checkedThread, cancellationToken: cancellationToken)
        .Select (d => d.FullName);
      int failedPlugins = 0;
      foreach (var byPlugin in activePackagePluginAssociations.GroupBy (a => a.Plugin)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadPluginsAsync: load plugin {byPlugin}");
        }
        try {
          await LoadPluginAsync (cancellationToken, byPlugin.Key, pluginDirectories, byPlugin, pluginFilter);
        }
        catch (Exception ex) {
          log.Error ($"LoadPluginsAsync: load of plugin {byPlugin} failed", ex);
          pluginStatusLog.Error ($"Load of {byPlugin} failed", ex);
          failedPlugins++;
        }
      }

      if (log.IsWarnEnabled) {
        if (0 == failedPlugins) {
          log.Debug ("LoadPluginsAsync: all the plugins were successfully loaded");
        }
        else {
          log.Warn ($"LoadPluginsAsync: {failedPlugins} plugins failed to load");
        }
      }
      if (pluginStatusLog.IsWarnEnabled && (0 != failedPlugins)) {
        pluginStatusLog.Warn ($"{failedPlugins} plugins were not loaded");
      }
    }

    bool LoadPlugin (IPlugin plugin, IEnumerable<string> pluginDirectories, IEnumerable<IPackagePluginAssociation> packagePluginAssociations, IPluginFilter pluginFilter)
    {
      Debug.Assert (null != plugin);
      Debug.Assert (null != packagePluginAssociations);

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadPlugin: load plugin {plugin.IdentifyingName}");
      }

      PluginDllLoader pluginDllLoader;
      try {
        pluginDllLoader = PluginDllLoader.LoadFromName (m_assemblyLoader, plugin.IdentifyingName, pluginDirectories, pluginFilter);
      }
      catch (Exception ex) {
        log.Error ($"LoadPlugin: while trying to load plugin {plugin.IdentifyingName}", ex);
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          log.Error ($"LoadPlugin: requres to exit, throw it", ex);
          throw;
        }
        pluginStatusLog.Fatal ($"{plugin.IdentifyingName}: LoadFromName returned exception (unexpected)", ex); // An exception should require to exit
        return false;
      }
      if (!pluginDllLoader.IsValid) {
        m_loadErrorPlugins.Add (pluginDllLoader);
        if (!pluginDllLoader.FilterMatch) {
          // Already logged in the PluginDllLoader
          if (log.IsInfoEnabled) {
            log.Info ($"LoadPlugin: do not load plugin {plugin.IdentifyingName} because the flags do not match the flag filter {pluginFilter.GetPluginFlag ()}");
          }
          TryUpdateFlag (plugin, pluginDllLoader.Plugin);
        }
        else {
          log.Error ($"LoadPlugin: the plugin {plugin} is not valid, return false");
        }
        return false;
      }
      var pluginDll = pluginDllLoader.Plugin;
      Debug.Assert (null != pluginDll); // Because IsValid

      // Check the flag
      if (pluginDll is IFlaggedPlugin flaggedPlugin) {
        if (!plugin.Flag.HasValue || !plugin.Flag.Value.Equals (flaggedPlugin.Flags)) {
          if (log.IsInfoEnabled) {
            log.Info ($"LoadPlugin: update the flag of the plugin {plugin.IdentifyingName} to {flaggedPlugin.Flags}");
          }
          TryUpdateFlag (plugin, flaggedPlugin.Flags);
        }
      }

      // Update of the plugin?
      if (plugin.NumVersion < pluginDll.Version) {
        if (log.IsInfoEnabled) {
          log.Info ($"LoadPlugin: about to upgrade the plugin {plugin.IdentifyingName} from version {plugin.NumVersion} to {pluginDll.Version}");
        }
        var updateResult = TryUpgradePlugin (plugin, pluginDll);
        if (!updateResult) {
          log.Error ($"LoadPlugin: upgrade failed for plugin {plugin.IdentifyingName}");
          return false;
        }
      }

      // Plugin configuration
      SetConfigurations (plugin, pluginDll, packagePluginAssociations);

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadPlugin: plugin {plugin.IdentifyingName} was successfully loaded");
      }
      m_activePlugins.Add (pluginDll);
      return true;
    }

    async Task<bool> LoadPluginAsync (CancellationToken cancellationToken, IPlugin plugin, IEnumerable<string> pluginDirectories, IEnumerable<IPackagePluginAssociation> packagePluginAssociations, IPluginFilter pluginFilter)
    {
      Debug.Assert (null != plugin);
      Debug.Assert (null != packagePluginAssociations);

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadPluginAsync: load plugin {plugin.IdentifyingName}");
      }

      PluginDllLoader pluginDllLoader;
      try {
        pluginDllLoader = await PluginDllLoader.LoadFromNameAsync (cancellationToken, m_assemblyLoader, plugin.IdentifyingName, pluginDirectories, pluginFilter);
      }
      catch (Exception ex) {
        log.Error ($"LoadPluginAsync: while trying to load plugin {plugin.IdentifyingName}", ex);
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          log.Error ($"LoadPluginAsync: exception requres to exit, throw it", ex);
          throw;
        }
        pluginStatusLog.Fatal ($"{plugin.IdentifyingName}: LoadFromName returned exception (unexpected)", ex); // An exception should require to exit
        return false;
      }
      if (!pluginDllLoader.IsValid) {
        m_loadErrorPlugins.Add (pluginDllLoader);
        if (!pluginDllLoader.FilterMatch) {
          // Already logged in the PluginDllLoader
          if (log.IsInfoEnabled) {
            log.Info ($"LoadPluginAsync: do not load plugin {plugin.IdentifyingName} because the flags do not match the flag filter {pluginFilter}");
          }
          await TryUpdateFlagAsync (plugin, pluginDllLoader.Plugin);
        }
        else {
          log.Error ($"LoadPluginAsync: the plugin {plugin.IdentifyingName} is not valid, return false");
        }
        return false;
      }
      var pluginDll = pluginDllLoader.Plugin;
      Debug.Assert (null != pluginDll); // Because IsValid

      // Check the flag
      if (pluginDll is IFlaggedPlugin flaggedPlugin) {
        if (!plugin.Flag.HasValue || !plugin.Flag.Value.Equals (flaggedPlugin.Flags)) {
          if (log.IsInfoEnabled) {
            log.Info ($"LoadPluginAsync: update the flag of the plugin {plugin.IdentifyingName} to {flaggedPlugin.Flags}");
          }
          await TryUpdateFlagAsync (plugin, flaggedPlugin.Flags);
        }
      }

      // Update of the plugin?
      if (plugin.NumVersion < pluginDll.Version) {
        if (log.IsInfoEnabled) {
          log.Info ($"LoadPluginAsync: about to upgrade the plugin {plugin.IdentifyingName} from version {plugin.NumVersion} to {pluginDll.Version}");
        }
        var updateResult = await TryUpgradePluginAsync (plugin, pluginDll);
        if (!updateResult) {
          log.Error ($"LoadPlugin: upgrade failed for plugin {plugin.IdentifyingName}");
          return false;
        }
      }

      // Plugin configuration
      SetConfigurations (plugin, pluginDll, packagePluginAssociations);

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadPluginAsync: plugin {plugin.IdentifyingName} was successfully loaded");
      }
      m_activePlugins.Add (pluginDll);
      return true;
    }

    void TryUpdateFlag (IPlugin plugin, IPluginDll pluginDll)
    {
      if (!plugin.Flag.HasValue && (null != pluginDll) && (pluginDll is IFlaggedPlugin flaggedPlugin)) {
        // But you can try to update the flag if the flag is not set it yet, to make the process
        // faster in the future
        if (!plugin.Flag.HasValue || !plugin.Flag.Value.Equals (flaggedPlugin.Flags)) {
          if (log.IsInfoEnabled) {
            log.Info ($"TryUpdateFlag: update the flag of the plugin {plugin.IdentifyingName} to {flaggedPlugin.Flags}");
          }
          TryUpdateFlag (plugin, flaggedPlugin.Flags);
        }
      }
    }

    void TryUpdateFlag (IPlugin plugin, PluginFlag pluginFlag)
    {
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("ExtensionManager.LoadPlugin.UpdateFlag", TransactionLevel.ReadCommitted)) {
            ModelDAOHelper.DAOFactory.PluginDAO.Lock (plugin);
            if (!plugin.Flag.HasValue || !plugin.Flag.Equals (pluginFlag)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"TryUpdateFlag: update the flag to {pluginFlag}");
              }
              plugin.Flag = pluginFlag;
              ModelDAOHelper.DAOFactory.PluginDAO.MakePersistent (plugin);
            }
            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ("UpdateFlag: exception", ex);
      }
    }

    async Task TryUpdateFlagAsync (IPlugin plugin, IPluginDll pluginDll)
    {
      if (!plugin.Flag.HasValue && (null != pluginDll) && (pluginDll is IFlaggedPlugin flaggedPlugin)) {
        // But you can try to update the flag if the flag is not set it yet, to make the process
        // faster in the future
        if (!plugin.Flag.HasValue || !plugin.Flag.Value.Equals (flaggedPlugin.Flags)) {
          if (log.IsInfoEnabled) {
            log.Info ($"TryUpdateFlagAsync: update the flag of the plugin {plugin.IdentifyingName} to {flaggedPlugin.Flags}");
          }
          await TryUpdateFlagAsync (plugin, flaggedPlugin.Flags);
        }
      }
    }

    async Task TryUpdateFlagAsync (IPlugin plugin, PluginFlag pluginFlag)
    {
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("ExtensionManager.LoadPlugin.UpdateFlag", TransactionLevel.ReadCommitted)) {
            ModelDAOHelper.DAOFactory.PluginDAO.Lock (plugin);
            if (!plugin.Flag.HasValue || !plugin.Flag.Equals (pluginFlag)) {
              log.Debug ($"TryUpdateFlagAsync: update the flag to {pluginFlag}");
              plugin.Flag = pluginFlag;
              await ModelDAOHelper.DAOFactory.PluginDAO.MakePersistentAsync (plugin);
            }
            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ("TryUpdateFlagAsync: exception", ex);
      }
    }

    bool TryUpgradePlugin (IPlugin plugin, IPluginDll pluginDll)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("ExtensionManager.LoadPlugin.Upgrade",
          TransactionLevel.ReadCommitted)) {
          try {
            ModelDAOHelper.DAOFactory.PluginDAO.Lock (plugin);
            pluginDll.Install (plugin.NumVersion);
            plugin.NumVersion = pluginDll.Version;
          }
          catch (Exception ex) {
            log.Error ($"TryUpgradePlugin: couldn't upgrade plugin {plugin.IdentifyingName}", ex);
            pluginStatusLog.Error ($"{plugin.IdentifyingName}: upgrade from {plugin.NumVersion} to {pluginDll.Version}", ex);
            transaction.Rollback ();
            return false;
          }
          transaction.Commit ();
        }
      }

      return true;
    }

    async Task<bool> TryUpgradePluginAsync (IPlugin plugin, IPluginDll pluginDll)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("ExtensionManager.LoadPlugin.Upgrade",
          TransactionLevel.ReadCommitted)) {
          try {
            await ModelDAOHelper.DAOFactory.PluginDAO.LockAsync (plugin);
            pluginDll.Install (plugin.NumVersion); // TODO: async version of Install
            plugin.NumVersion = pluginDll.Version;
          }
          catch (Exception ex) {
            log.Error ($"TryUpgradePlugin: couldn't upgrade plugin {plugin.IdentifyingName}", ex);
            pluginStatusLog.Error ($"{plugin.IdentifyingName}: upgrade from {plugin.NumVersion} to {pluginDll.Version}", ex);
            transaction.Rollback ();
            return false;
          }
          transaction.Commit ();
        }
      }

      return true;
    }

    void SetConfigurations (IPlugin plugin, IPluginDll pluginDll, IEnumerable<IPackagePluginAssociation> packagePluginAssociations)
    {
      foreach (var packagePluginAssociation in packagePluginAssociations) {
        SetConfiguration (plugin, pluginDll, packagePluginAssociation);
      }
    }

    void SetConfiguration (IPlugin plugin, IPluginDll pluginDll, IPackagePluginAssociation packagePluginAssociation)
    {
      var confErrors = pluginDll.GetConfigurationErrors (packagePluginAssociation.Parameters);
      if (confErrors != null && confErrors.Any ()) {
        log.ErrorFormat ("SetConfigurations: " +
                        "configuration error(s) found for package {0} and plugin {1}: {2} " +
                        "=> skip this configuration",
                        packagePluginAssociation.Package.IdentifyingName, plugin.IdentifyingName,
                        String.Join (";\n- ", confErrors.ToArray ()));
        pluginStatusLog.ErrorFormat ("{0}:instance {1}: configuration error",
          plugin.IdentifyingName, packagePluginAssociation.Id);
      }
      else { // Configuration ok
        if (pluginStatusLog.IsDebugEnabled) {
          pluginStatusLog.Debug ($"{plugin.IdentifyingName}:instance {packagePluginAssociation.Id}: successfully added");
        }
        pluginDll.AddInstance (packagePluginAssociation);
      }
    }
    #endregion // Methods
  }
}
