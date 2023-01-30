// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Lemoine.FileRepository;
using Lemoine.Info;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin.PluginDirectories
{
  /// <summary>
  /// Abstract class to synchronize and load plugin directories only once (eager load)
  /// 
  /// Threadsafe
  /// </summary>
  public abstract class Directories : IPluginDirectories
  {
    /// <summary>
    /// Get an alternative plugins directories (for tests for example)
    /// 
    /// Semi-colon (;) separated list
    /// 
    /// They may be absolute or relative to the program directory
    /// 
    /// If emtpy, it is skipped
    /// </summary>
    static readonly string ALTERNATIVE_PLUGINS_DIRECTORIES_KEY = "Extensions.AlternativePluginsDirectories";
    static readonly string ALTERNATIVE_PLUGINS_DIRECTORIES_DEFAULT = "";

    #region Members
    readonly IAssemblyLoader m_assemblyLoader;

    bool? m_pluginUserDirectoryActive = null;
    bool m_pluginDirectoriesLoaded = false;
    readonly SemaphoreSlim m_pluginDirectoriesSemaphore = new SemaphoreSlim (1, 1);

    int m_synchronizationStatus = 0; // 1: in progress, 2: done

    readonly IList<DirectoryInfo> m_pluginDirectories = new List<DirectoryInfo> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PluginDirectories).FullName);
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    #region Getters / Setters
    /// <summary>
    /// Associated assembly loader
    /// </summary>
    protected IAssemblyLoader AssemblyLoader => m_assemblyLoader;

    /// <summary>
    /// Optional: synchronization timeout
    /// </summary>
    public TimeSpan? SynchronizationTimeout { get; set; } = null;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="assemblyLoader">not null</param>
    protected Directories (IAssemblyLoader assemblyLoader)
    {
      Debug.Assert (null != assemblyLoader);

      m_assemblyLoader = assemblyLoader;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    protected virtual ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// Plugin directories
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginNames">nullable</param>
    /// <param name="checkedThread">nullable</param>
    /// <param name="cancellationToken">Optional</param>
    public IEnumerable<DirectoryInfo> GetDirectories (bool pluginUserDirectoryActive, IEnumerable<string> pluginNames = null, Lemoine.Threading.IChecked checkedThread = null, CancellationToken? cancellationToken = null)
    {
      CheckCache (pluginUserDirectoryActive);
      LoadPluginDirectories (pluginUserDirectoryActive, pluginNames, checkedThread, cancellationToken: cancellationToken);
      return m_pluginDirectories.Distinct ();
    }

    void CheckCache (bool pluginUserDirectoryActive)
    {
      if (m_pluginUserDirectoryActive.HasValue
        && (pluginUserDirectoryActive != m_pluginUserDirectoryActive.Value)) { // New pluginUserDirectoryActive parameter
        GetLogger ().Warn ("CheckCache: pluginUserDirectoryActive parameter changed, reset the cache");
        m_pluginDirectoriesLoaded = false;
      }
      m_pluginUserDirectoryActive = pluginUserDirectoryActive;
    }

    void LoadPluginDirectories (bool pluginUserDirectoryActive, IEnumerable<string> pluginNames, Lemoine.Threading.IChecked checkedThread = null, CancellationToken? cancellationToken = null)
    {
      if (m_pluginDirectoriesLoaded) {
        return;
      }

      SynchronizePluginUserDirectory (pluginUserDirectoryActive, pluginNames, checkedThread, this.SynchronizationTimeout, cancellationToken: cancellationToken);

      if (m_pluginDirectoriesLoaded) {
        return;
      }

      if (null != checkedThread) {
        checkedThread.SetActive ();
      }

      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_pluginDirectoriesSemaphore)) {
        if (m_pluginDirectoriesLoaded) {
          return;
        }

        // 1. AlternativePluginDirectories
        string alternativePluginsDirectories = Lemoine.Info.ConfigSet
          .LoadAndGet<string> (ALTERNATIVE_PLUGINS_DIRECTORIES_KEY, ALTERNATIVE_PLUGINS_DIRECTORIES_DEFAULT);
        if (!string.IsNullOrEmpty (alternativePluginsDirectories)) {
          var directories = alternativePluginsDirectories.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
          foreach (var directory in directories) {
            if (Path.IsPathRooted (directory)) {
              ValidateAndAddDirectory (directory);
            }
            else {
              var programDirectory = Lemoine.Info.ProgramInfo.AbsoluteDirectory;
              if (null == programDirectory) {
                GetLogger ().Error ($"LoadPluginDirectory: program directory is now known for relative alternative directory {directory}");
              }
              else { // null != programDirectory
                var absolutePath = Path.Combine (programDirectory, directory);
                ValidateAndAddDirectory (absolutePath);
              }
            }
          }
        }

        // 2. 'CommonConfigDirectory'/plugins.d
        {
          var commonConfigDirectory = Lemoine.Info.PulseInfo.CommonConfigurationDirectory;
          Debug.Assert (!string.IsNullOrEmpty (commonConfigDirectory));
          var commonConfigPluginPath = Path.Combine (commonConfigDirectory, "plugins.d");
          if (!Directory.Exists (commonConfigPluginPath)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"LoadPluginDirectories: Create directory {commonConfigPluginPath}");
            }
            Directory.CreateDirectory (commonConfigPluginPath);
          }
          ValidateAndAddDirectory (commonConfigPluginPath);
        }

        // 3. PluginUserDir
        if (pluginUserDirectoryActive) {
          var pluginUserDirectoryPath = GetLocalSyncPluginsDirectory ();
          ValidateAndAddDirectory (pluginUserDirectoryPath);
        }

        // 4. Program directory
        var programAbsoluteDirectory = ProgramInfo.AbsoluteDirectory;
        if (null == programAbsoluteDirectory) {
          log.Error ($"LoadPluginDirectories: program directory is not known");
        }
        else {
          var programDirInfo = new DirectoryInfo (programAbsoluteDirectory);
          Debug.Assert (programDirInfo.Exists);
          if (!programDirInfo.Exists) {
            log.Fatal ($"LoadPluginDirectories: {programDirInfo} does not exist");
          }
          else {
            AddDirectory (programDirInfo);
          }
        }

        // 5. Installation directory / l_ctr/pfrdata/plugins_(synchronized|core)
        var pfrDataDirectory = PulseInfo.PfrDataDir;
        if (null != pfrDataDirectory) {
          var pfrdataPluginsPath = Path.Combine (pfrDataDirectory, this.AssemblyLoader.GetPluginsFileRepoOsPath ());
          var pfrdataPluginsDirInfo = new DirectoryInfo (pfrdataPluginsPath);
          if (!pfrdataPluginsDirInfo.Exists) {
            log.Info ($"LoadPluginDirectories: {pfrdataPluginsPath} does not exist");
          }
          else {
            AddDirectory (pfrdataPluginsDirInfo);
          }
        }

        LoadAdditionalPluginDirectories (pluginUserDirectoryActive, pluginNames, checkedThread);

        m_pluginDirectoriesLoaded = true;
      }

      if (null != checkedThread) {
        checkedThread.SetActive ();
      }
    }

    /// <summary>
    /// To override to load additional plugin directories
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginNames"></param>
    /// <param name="checkedThread"></param>
    protected virtual void LoadAdditionalPluginDirectories (bool pluginUserDirectoryActive, IEnumerable<string> pluginNames, Lemoine.Threading.IChecked checkedThread)
    {
    }

    /// <summary>
    /// Should it include the application name in the synchronization path ?
    /// </summary>
    /// <returns></returns>
    protected virtual bool IncludeApplicationName ()
    {
      return true;
    }

    /// <summary>
    /// To override to get a specific directory
    /// </summary>
    /// <returns></returns>
    protected abstract string GetSpecificDirectoryName ();

    string GetLocalSyncPluginsDirectory ()
    {
      var pfrPluginDirectoryName = m_assemblyLoader.GetPluginsFileRepoOsPath ();
      var path = pfrPluginDirectoryName;
      if (IncludeApplicationName ()) {
        string applicationName;
        using (var currentProcess = System.Diagnostics.Process.GetCurrentProcess ()) {
          applicationName = currentProcess.ProcessName;
        }
        if (applicationName.Equals ("dotnet")) {
          log.Info ($"GetLocalSyncPluginsDirectory: process name is {applicationName} => use {Lemoine.Info.ProgramInfo.Name} instead");
          applicationName = Lemoine.Info.ProgramInfo.Name;
        }
        // Note: previously Lemoine.Info.ProgramInfo.Name, but it returned Lem_CncConsole instead of Lem_CncConsole-1
        if (!string.IsNullOrEmpty (applicationName)) {
          path = System.IO.Path.Combine (path, applicationName);
        }
      }
      path = Path.Combine (PulseInfo.LocalConfigurationDirectory, path);
      path = Path.Combine (path, GetSpecificDirectoryName ());
      return path;
    }

    /// <summary>
    /// Default filter, that is based on the plugin names
    /// </summary>
    /// <param name="fileNames"></param>
    /// <param name="fileName"></param>
    /// <param name="pluginNames"></param>
    /// <returns></returns>
    protected virtual bool Filter (IEnumerable<string> fileNames, string fileName, IEnumerable<string> pluginNames)
    {
      if (null != pluginNames) {
        var fullPluginFileNames = pluginNames
          .Select (f => "Lemoine.Plugin." + f + ".dll");
        return fullPluginFileNames.Contains (fileName);
      }
      else {
        return true;
      }
    }

    void SynchronizePluginUserDirectory (bool pluginUserDirectoryActive, IEnumerable<string> pluginNames, Lemoine.Threading.IChecked checkedThread = null, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
    {
      if (null != checkedThread) {
        checkedThread.SetActive ();
      }

      if (!pluginUserDirectoryActive) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("SynchronizePluginUserDirectory: the plugin user is not active, nothing to do");
        }
        return;
      }

      var synchronizationStatus = System.Threading.Interlocked.CompareExchange (ref m_synchronizationStatus, 1, 0);
      switch (synchronizationStatus) {
      case 0: // Not done, do it
        try {
          if (null != checkedThread) {
            checkedThread.SetActive ();
          }
          // Plugin user directory section
          // Synchronize it if it has not been initialized yet
          var pluginUserDirectoryPath = GetLocalSyncPluginsDirectory ();
          bool filter ( IEnumerable<string> fileNames, string f ) => Filter (fileNames, f, pluginNames);
          bool synchronizationResult = Synchronize (pluginUserDirectoryPath, filter, checkedThread, timeout, cancellationToken: cancellationToken);
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"SynchronizePluginUserDirectory: plugin user dir {pluginUserDirectoryPath}. Synchronization result: {synchronizationResult}");
          }
          if (synchronizationResult) {
            if (pluginStatusLog.IsInfoEnabled) {
              pluginStatusLog.Info ($"Plugin user directory {pluginUserDirectoryPath} was successfully synchronized");
            }
          }
          else {
            pluginStatusLog.Error ($"Error in the synchronization of the plugin user directory {pluginUserDirectoryPath}");
          }
        }
        catch (Exception ex) {
          GetLogger ().Error ("SynchronizePluginUserDirectory: exception", ex);
          throw;
        }
        finally {
          System.Threading.Interlocked.CompareExchange (ref m_synchronizationStatus, 2, 1); // Switch to 2 (completed or exception)
        }
        return;
      case 1: // In progress, wait for the completion
        GetLogger ().DebugFormat ("SynchronizePluginUserDirectory: waiting for completion");
        while (1 == m_synchronizationStatus) { // Still in progress
          if (null != checkedThread) {
            checkedThread.SetActive ();
          }
          System.Threading.Thread.Sleep (100);
        }
        return;
      case 2: // Completed
        return;
      default:
        GetLogger ().Fatal ($"SynchronizePluginUserDirectory: unexpected synchronization status {synchronizationStatus}");
        throw new InvalidOperationException ();
      }
    }

    void ValidateAndAddDirectory (string directoryPath)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"ValidateAndAddDirectory: path={directoryPath}");
      }

      if (string.IsNullOrEmpty (directoryPath)) {
        GetLogger ().Error ("ValidateAndAddDirectory: path is null or empty, skip it");
        return;
      }

      var directoryInfo = new DirectoryInfo (directoryPath);
      if (!directoryInfo.Exists) {
        GetLogger ().Error ($"ValidateAndAddDirectory: {directoryPath} does not exist, skip it");
        return;
      }

      AddDirectory (directoryInfo);
    }

    /// <summary>
    /// Add a directory to look at
    /// </summary>
    /// <param name="directoryInfo"></param>
    protected void AddDirectory (DirectoryInfo directoryInfo)
    {
      m_pluginDirectories.Add (directoryInfo);
    }

    bool Synchronize (string path, Func<IEnumerable<string>, string, bool> filter, Lemoine.Threading.IChecked checkedThread = null, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
    {
      Debug.Assert (!string.IsNullOrEmpty (path));
      var directory = new DirectoryInfo (path);
      if (!directory.Exists) {
        try {
          directory.Create ();
        }
        catch (Exception ex) {
          GetLogger ().Error ($"Synchronize: couldn't create the plugin user dir {directory}", ex);
          return false;
        }
      }
      var fullPath = directory.FullName;

      string distantDirectory = m_assemblyLoader.GetPluginsFileRepoPath ();
      SynchronizationStatus status;
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_pluginDirectoriesSemaphore)) {
        status = FileRepoClient.ForceSynchronize (distantDirectory,
                                                  fullPath,
                                                  filter,
                                                  checkedThread,
                                                  timeout,
                                                  cancellationToken: cancellationToken);
      }

      if (SynchronizationStatus.SYNCHRONIZATION_FAILED == status) {
        GetLogger ().Error ($"Synchronize: {distantDirectory} -> {distantDirectory} failed with status {status}");
      }
      else if (log.IsDebugEnabled) {
        GetLogger ().Debug ($"Synchronize: {distantDirectory} -> {directory} was successful with status {status}");
      }

      return status == SynchronizationStatus.ALREADY_SYNCHRONIZED ||
        status == SynchronizationStatus.SYNCHRONIZATION_DONE;
    }
    #endregion // Methods
  }
}
