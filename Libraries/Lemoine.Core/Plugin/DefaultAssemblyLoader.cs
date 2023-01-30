// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.Core.Plugin
{
  /// <summary>
  /// AssemblyLoader
  /// </summary>
  public class DefaultAssemblyLoader
    : IAssemblyLoader
  {
    const string PLUGINS_FILE_REPO_PATH_KEY = "AssemblyLoader.PluginsFileRepoPath";
    const string PLUGINS_FILE_REPO_PATH_DEFAULT =
#if NETCOREAPP
      "plugins_core";
#else // !NETCOREAPP
      "plugins_synchronized";
#endif // !NETCOREAPP

    readonly ILog log = LogManager.GetLogger (typeof (DefaultAssemblyLoader).FullName);
    readonly ILog successLog = LogManager.GetLogger ($"{typeof (DefaultAssemblyLoader).FullName}.Success");

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultAssemblyLoader ()
    {
    }
#endregion // Constructors

    /// <summary>
    /// <see cref="IAssemblyLoader"/>
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Assembly LoadFromPath (string path)
    {
      return LoadFromPath (path, true);
    }

#if (NETCOREAPP && !NETCOREAPP3_0 && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
    // Note: see https://docs.microsoft.com/fr-fr/dotnet/core/tutorials/creating-app-with-plugin-support
    Assembly LoadFromAssemblyNameWithContext (string path, params string[] additionalPaths)
    {
      var fileNameWithoutExtension = Path.GetFileNameWithoutExtension (path);
      var loadContext = new PluginLoadContext (path, additionalPaths);
      if (log.IsDebugEnabled) {
        log.Debug ($"LoadFromAssemblyNameWithContext: context={path} name={fileNameWithoutExtension}");
      }
      return loadContext.LoadFromAssemblyName (new AssemblyName (fileNameWithoutExtension));
    }

    Assembly LoadFromAssemblyPathWithContext (string path, params string[] additionalPaths)
    {
      var loadContext = new PluginLoadContext (path, additionalPaths);
      if (log.IsDebugEnabled) {
        log.Debug ($"LoadFromAssemblyPathWithContext: context=path={path}");
      }
      return loadContext.LoadFromAssemblyPath (path);
    }
#endif // NETCOREAPP >= 3_1

    /// <summary>
    /// <see cref="IAssemblyLoader"/>
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="topDirectoryOnly"></param>
    /// <param name="directories"></param>
    /// <returns></returns>
    public Assembly LoadFromName (AssemblyName assemblyName, bool topDirectoryOnly, params string[] directories)
    {
      return LoadFromName (assemblyName.Name, topDirectoryOnly: topDirectoryOnly, directories);
    }

    /// <summary>
    /// <see cref="IAssemblyLoader"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="topDirectoryOnly"></param>
    /// <param name="directories"></param>
    /// <returns></returns>
    public Assembly LoadFromName (string name, bool topDirectoryOnly, params string[] directories)
    {
      return LoadFromName (name, directories.Where (d => !string.IsNullOrEmpty (d)), topDirectoryOnly: topDirectoryOnly);
    }

    /// <summary>
    /// <see cref="IAssemblyLoader"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="directories"></param>
    /// <param name="topDirectoryOnly"></param>
    /// <returns></returns>
    public Assembly LoadFromName (string name, IEnumerable<string> directories, bool topDirectoryOnly = false)
    {
#if !(NETCOREAPP && !NETCOREAPP3_0 && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
      try {
        return Assembly.Load (name);
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadFromName: Direct load failed for {name}", ex);
        }
      }
#endif // !NETCOREAPP3_1 && !NET5_0_OR_GREATER

      var possiblePaths = GetPossiblePaths (name, directories, topDirectoryOnly: topDirectoryOnly);
      foreach (var possiblePath in possiblePaths) {
        try {
          if (log.IsInfoEnabled) {
            log.Info ($"LoadFromName: try loading from path {possiblePath}");
          }
          var assembly = LoadFromPath (possiblePath, false);
          if (successLog.IsInfoEnabled) {
            successLog.Info ($"LoadFromName: {name} => {possiblePath}");
          }
          return assembly;
        }
        catch (Exception ex) {
          if (log.IsDebugEnabled) {
            log.Debug ($"LoadFromName: GetPossiblePaths for path {possiblePath} failed", ex);
          }
        }
      }

#if (NETCOREAPP && !NETCOREAPP3_0 && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
      if (log.IsWarnEnabled) {
        log.Warn ($"LoadFromName: load from path failed with all possible paths. Try with direct load from name");
      }
      try {
        var assembly = Assembly.Load (name);
        if (log.IsDebugEnabled) {
          successLog.Debug ($"LoadFromName: direct load of {name}");
        }
        return assembly;
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadFromName: Direct load failed for {name}", ex);
        }
      }
#endif // NETCOREAPP >= 3.1

      if (log.IsErrorEnabled) {
        log.Error ($"LoadFromName: no valid path was found for assembly name {name}");
        log.Error ($"LoadFromName: tested paths for {name} were {string.Join (";", possiblePaths)}");
      }
      throw new Exception ("No valid path for assembly name " + name);
    }

    IEnumerable<string> GetPossiblePaths (string name, IEnumerable<string> directories, bool topDirectoryOnly = false)
    {
      IList<string> topOnlyDirectories = new List<string> ();
      if (!topDirectoryOnly) {
        var pulseServerDirectory = Lemoine.Info.PulseInfo.PulseServerInstallationDirectory;
        if (!string.IsNullOrEmpty (pulseServerDirectory)) {
          topOnlyDirectories.Add (pulseServerDirectory);
          var coreDirectory = Path.Combine (pulseServerDirectory, "core");
          if (Directory.Exists (coreDirectory)) {
            topOnlyDirectories.Add (coreDirectory);
          }
        }
        var installationDirectory = Lemoine.Info.PulseInfo.InstallationDir;
        if (!string.IsNullOrEmpty (installationDirectory)) {
          topOnlyDirectories.Add (installationDirectory);
          var coreDirectory = Path.Combine (installationDirectory, "core");
          if (Directory.Exists (coreDirectory)) {
            topOnlyDirectories.Add (coreDirectory);
          }
        }
      }
      foreach (var directory in directories) {
        if (Directory.Exists (directory)) {
          SearchOption searchOption;
          if (topDirectoryOnly) {
            searchOption = SearchOption.TopDirectoryOnly;
          }
          else {
            if (topOnlyDirectories.Any (d => Path.Equals (d, directory))) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetPossiblePaths: directory {directory} is a top only directory");
              }
              searchOption = SearchOption.TopDirectoryOnly;
            }
            else {
              searchOption = SearchOption.AllDirectories;
            }
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPossiblePaths: search {name}.dll in {directory}");
          }
          var files = Directory.GetFiles (directory, name + ".dll", searchOption);
          foreach (var file in files) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetPossiblePaths: return {file}");
            }
            yield return file;
          }
        }
      }
    }

    Assembly LoadFromPath (string path, bool tryLoadFromName)
    {
#if (NETCOREAPP && !NETCOREAPP3_0 && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
      var programPath = Lemoine.Info.ProgramInfo.AbsolutePath;

      try {
        var assembly = LoadFromAssemblyNameWithContext (path, programPath);
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadFromPath: LoadFromAssemblyNameWithContext successful with path={path} programPath={programPath}");
        }
        return assembly;
      }
      catch (Exception ex) {
        log.Warn ($"LoadFromPath: LoadFromAssemblyNameWithContext failed for {path}", ex);
      }

      try {
        var assembly = LoadFromAssemblyPathWithContext (path, programPath);
        if (log.IsDebugEnabled) {
          log.Debug ($"LoadFromPath: LoadFromAssemblyPathWithContext successful with path={path} programPath={programPath}");
        }
        return assembly;
      }
      catch (Exception ex) {
        log.Warn ($"LoadFromPath: LoadFromAssemblyPathWithContext failed for {path}", ex);
      }
#endif // NETCOREAPP >= 3.1

      if (tryLoadFromName) {
        try {
          var fileNameWithoutExtension = Path.GetFileNameWithoutExtension (path);
          var assembly = Assembly.Load (fileNameWithoutExtension);
#if (NETCOREAPP && !NETCOREAPP3_0 && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
          if (log.IsWarnEnabled) {
            log.Warn ($"LoadFromPath: load from name {fileNameWithoutExtension} directly (not with context), return {assembly}");
          }
#endif // NETCOREAPP >= 3.1
          return assembly;
        }
        catch (Exception ex) {
          log.Debug ($"LoadFromPath: Load failed for {path}", ex);
        }
      }

      try {
        var assembly = Assembly.LoadFrom (path);
#if (NETCOREAPP && !NETCOREAPP3_0 && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
        if (log.IsWarnEnabled) {
          log.Warn ($"LoadFromPath: load from path {path} (not with context) successful, return {assembly}");
        }
#endif // NETCOREAPP >= 3.1
        return assembly;
      }
      catch (Exception ex) {
        log.Warn ($"LoadFromPath: LoadFrom failed for {path}, try LoadFile", ex);
      }

      try {
        var assembly = Assembly.LoadFile (path);
#if (NETCOREAPP && !NETCOREAPP3_0 && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
        if (log.IsWarnEnabled) {
          log.Warn ($"LoadFromPath: load directly file {path} (not with context) successful, return {assembly}");
        }
#endif // NETCOREAPP >= 3.1
        return assembly;
      }
      catch (Exception ex) {
        log.Warn ($"LoadFromPath: LoadFile failed for {path}", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IAssemblyLoader"/>
    /// </summary>
    /// <returns></returns>
    public string GetPluginsFileRepoPath ()
    {
      return Lemoine.Info.ConfigSet
        .LoadAndGet (PLUGINS_FILE_REPO_PATH_KEY, PLUGINS_FILE_REPO_PATH_DEFAULT);
    }
  }
}
