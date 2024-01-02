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
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Plugin;
using System.Threading;
using Pulse.Extensions;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Class to synchronize and load the plugin directories only once (eager load)
  /// with no database access
  /// 
  /// Threadsafe
  /// </summary>
  public class NHibernatePluginsLoader : INHibernatePluginsLoader
  {
    #region Members
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IPluginDirectories m_pluginDirectories;

    bool? m_pluginUserDirectoryActive;
    PluginFlag? m_pluginFlagFilter;
    int m_loadStatus = 0; // 1: in progress, 2: done
    int m_loadingThreadId = 0;

    readonly IList<IPluginDll> m_activePlugins = new List<IPluginDll> ();
    readonly IList<PluginDllLoader> m_loadErrorPlugins = new List<PluginDllLoader> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (NHibernatePluginsLoader).FullName);
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
    public NHibernatePluginsLoader (IAssemblyLoader assemblyLoader, IPluginSynchronizationTimeoutProvider pluginSynchronizationTimeoutProvider = null)
    {
      Debug.Assert (null != assemblyLoader);

      m_assemblyLoader = assemblyLoader;
      var pluginDirectories = new Plugin.PluginDirectories.NHibernatePluginDirectories (assemblyLoader);
      if (null != pluginSynchronizationTimeoutProvider) {
        pluginDirectories.SynchronizationTimeout = pluginSynchronizationTimeoutProvider.PluginSynchronizationTimeout;
      };
      m_pluginDirectories = pluginDirectories;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="IPluginsLoader"/>
    /// </summary>
    /// <param name="checkedThread"></param>
    public void Clear (Lemoine.Threading.IChecked checkedThread)
    {
      log.Fatal ("Clear: not implemented since there is no reason to clear the NHibernate plugins");
      throw new NotImplementedException ("No reason to clear the NHibernate plugins");
    }

    void CheckCache (bool pluginUserDirectoryActive, IPluginFilter pluginFilter)
    {
      if (m_pluginUserDirectoryActive.HasValue
        && (pluginUserDirectoryActive != m_pluginUserDirectoryActive.Value)) { // New pluginUserDirectoryActive parameter
        log.WarnFormat ("CheckCache: pluginUserDirectoryActive parameter changed, reset the cache");
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
    public async System.Threading.Tasks.Task LoadAsync (CancellationToken cancellationToken, bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
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
    public IEnumerable<IPluginDllLoader> GetLoadErrorPlugins ()
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
          throw new InvalidOperationException ("InstanceLoadAsync can be run");
        }
        try {
          pluginStatusLog.Info ("Load the plugins requested without the database connection");
          pluginStatusLog.DebugFormat ("Load of the plugins without the database connection requested by {0}", System.Environment.StackTrace);
          if (null != checkedThread) {
            checkedThread.SetActive ();
          }
          var directories = m_pluginDirectories.GetDirectories (pluginUserDirectoryActive, checkedThread: checkedThread)
            .Select (directory => directory.FullName);
          LoadPlugins (directories, pluginFilter);
        }
        catch (Exception ex) {
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
        log.FatalFormat ("InstanceLoad: unexpected status {0}", loadStatus);
        throw new InvalidOperationException ();
      }
    }

    async System.Threading.Tasks.Task InstanceLoadAsync (CancellationToken cancellationToken, bool pluginUserDirectoryActive, IPluginFilter pluginFilter, Lemoine.Threading.IChecked checkedThread)
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
          throw new InvalidOperationException ("InstanceLoadAsync can be run");
        }
        m_loadingThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        try {
          pluginStatusLog.Info ("Load the plugins requested without the database connection");
          pluginStatusLog.DebugFormat ("Load of the plugins without the database connection requested by {0}", System.Environment.StackTrace);
          if (null != checkedThread) {
            checkedThread.SetActive ();
          }
          var directories = m_pluginDirectories.GetDirectories (pluginUserDirectoryActive, checkedThread: checkedThread)
            .Select (directory => directory.FullName);
          await LoadPluginsAsync (directories, pluginFilter);
        }
        catch (Exception ex) {
          log.Error ("InstanceLoadAsync: exception", ex);
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
          if (log.IsDebugEnabled) {
            log.Debug ("InstanceLoadAsync: current loading thread => return");
          }
          return;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"InstanceLoadAsync: waiting for completion, status={m_loadStatus} at {System.Environment.StackTrace}");
          }
          while (1 == m_loadStatus) { // Still in progress
            if (null != checkedThread) {
              checkedThread.SetActive ();
            }
            await System.Threading.Tasks.Task.Delay (100);
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"InstanceLoadAsync: completed status={m_loadStatus}");
          }
          return;
        }
      case 2: // Completed
        return;
      default:
        log.Fatal ($"InstanceLoadAsync: unexpected status {loadStatus}");
        throw new InvalidOperationException ();
      }
    }

    void LoadPlugins (IEnumerable<string> directories, IPluginFilter pluginFilter)
    {
      m_loadErrorPlugins.Clear ();
      m_activePlugins.Clear ();

      GetAndLoadPluginDlls (directories, pluginFilter);
    }

    async System.Threading.Tasks.Task LoadPluginsAsync (IEnumerable<string> directories, IPluginFilter pluginFilter)
    {
      m_loadErrorPlugins.Clear ();
      m_activePlugins.Clear ();

      await GetAndLoadPluginDllsAsync (directories, pluginFilter);
    }

    void GetAndLoadPluginDlls (IEnumerable<string> directories, IPluginFilter pluginFilter)
    {
      string pulseServerDirectory = Lemoine.Info.PulseInfo.MainServerInstallationDirectory;
      foreach (var directory in directories.Distinct ()) {
        SearchOption searchOption;
        if ((null != pulseServerDirectory) && Path.Equals (pulseServerDirectory, directory)) {
          log.WarnFormat ("GetPluginDlls: search directory is the Pulse directory");
          searchOption = SearchOption.TopDirectoryOnly;
        }
        else {
          searchOption = SearchOption.AllDirectories;
        }
        GetAndLoadPluginDlls (directory, searchOption, pluginFilter);
      }
    }

    async System.Threading.Tasks.Task GetAndLoadPluginDllsAsync (IEnumerable<string> directories, IPluginFilter pluginFilter)
    {
      string pulseServerDirectory = Lemoine.Info.PulseInfo.MainServerInstallationDirectory;
      // TODO: get and load the plugin dlls in parallel
      foreach (var directory in directories.Distinct ()) {
        SearchOption searchOption;
        if ((null != pulseServerDirectory) && Path.Equals (pulseServerDirectory, directory)) {
          log.WarnFormat ("GetPluginDlls: search directory is the Pulse directory");
          searchOption = SearchOption.TopDirectoryOnly;
        }
        else {
          searchOption = SearchOption.AllDirectories;
        }
        await GetAndLoadPluginDllsAsync (directory, searchOption, pluginFilter);
      }
    }

    void GetAndLoadPluginDlls (string directory, SearchOption searchOption, IPluginFilter pluginFilter)
    {
      var nhibernateExtensionFiles = Directory.GetFiles (directory, "*.nhibernateextension", searchOption);
      foreach (var nhibernateExtensionFile in nhibernateExtensionFiles.Distinct ()) {
        var pluginIdentifyingName = Path.GetFileNameWithoutExtension (nhibernateExtensionFile);
        if (m_activePlugins.Any (p => p.IdentifyingName.Equals (pluginIdentifyingName, StringComparison.InvariantCultureIgnoreCase))) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAndLoadPluginDlls: plugin {pluginIdentifyingName} has already been loaded, skip it");
          }
          continue;
        }
        var fileName = "Lemoine.Plugin." + pluginIdentifyingName + ".dll";
        var file = nhibernateExtensionFile.Replace (Path.GetFileName (nhibernateExtensionFile),
          fileName);
        if (!File.Exists (file)) {
          log.Error ($"GetAndLoadPluginDlls: no plugin file {file}");
          continue;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"GetAndLoadPluginDlls: try to load {file}");
        }
        var pluginDllLoader = PluginDllLoader.LoadDll (m_assemblyLoader, file, pluginFilter);
        if (pluginDllLoader.IsValid) {
          Debug.Assert (pluginDllLoader.Status.Equals (PluginLoadStatus.Success));
          var plugin = pluginDllLoader.Plugin;
          m_activePlugins.Add (plugin);
        }
        else {
          m_loadErrorPlugins.Add (pluginDllLoader);
          if (!pluginDllLoader.FilterMatch) {
            log.Info ($"GetAndLoadPluginDlls: do not load plugin {file} because the flags do not match the flag filter {pluginFilter}");
          }
          else {
            log.Error ($"GetPluginDlls: plugin {file} is not valid");
          }
        }
      }
    }

    async System.Threading.Tasks.Task GetAndLoadPluginDllsAsync (string directory, SearchOption searchOption, IPluginFilter pluginFilter)
    {
      var nhibernateExtensionFiles = Directory.GetFiles (directory, "*.nhibernateextension", searchOption);
      foreach (var nhibernateExtensionFile in nhibernateExtensionFiles.Distinct ()) {
        var pluginIdentifyingName = Path.GetFileNameWithoutExtension (nhibernateExtensionFile);
        if (m_activePlugins.Any (p => p.IdentifyingName.Equals (pluginIdentifyingName, StringComparison.InvariantCultureIgnoreCase))) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAndLoadPluginDllsAsync: plugin {pluginIdentifyingName} has already been loaded, skip it");
          }
          continue;
        }
        var fileName = "Lemoine.Plugin." + pluginIdentifyingName + ".dll";
        var file = nhibernateExtensionFile.Replace (Path.GetFileName (nhibernateExtensionFile),
          fileName);
        if (!File.Exists (file)) {
          log.Error ($"GetAndLoadPluginDllsAsync: no plugin file {file}");
          continue;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"GetAndLoadPluginDllsAsync: try to load {file}");
        }
        var pluginDllLoader = await PluginDllLoader.LoadDllAsync (m_assemblyLoader, file, pluginFilter);
        if (pluginDllLoader.IsValid) {
          Debug.Assert (pluginDllLoader.Status.Equals (PluginLoadStatus.Success));
          var plugin = pluginDllLoader.Plugin;
          m_activePlugins.Add (plugin);
        }
        else {
          m_loadErrorPlugins.Add (pluginDllLoader);
          if (!pluginDllLoader.FilterMatch) {
            log.InfoFormat ($"GetAndLoadPluginDllsAsync: do not load plugin {file} because the flags do not match the flag filter {pluginFilter}");
          }
          else {
            log.Error ($"GetAndLoadPluginDllsAsync: plugin {file} is not valid");
          }
        }
      }
    }
    #endregion // Methods
  }
}
