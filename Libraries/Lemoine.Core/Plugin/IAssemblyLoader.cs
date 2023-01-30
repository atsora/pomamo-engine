// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lemoine.Core.Plugin
{
  /// <summary>
  /// Interface for an assembly loader
  /// </summary>
  public interface IAssemblyLoader
  {
    /// <summary>
    /// Load an assembly given its path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Assembly LoadFromPath (string path);

    /// <summary>
    /// Load an assembly given its name and a list of possible directories
    /// 
    /// Empty directories are excluded
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="topDirectoryOnly"></param>
    /// <param name="directories"></param>
    /// <returns></returns>
    Assembly LoadFromName (AssemblyName assemblyName, bool topDirectoryOnly, params string[] directories);

    /// <summary>
    /// Load an assembly given its name and a list of possible directories
    /// 
    /// Empty directories are excluded
    /// </summary>
    /// <param name="name"></param>
    /// <param name="topDirectoryOnly"></param>
    /// <param name="directories"></param>
    /// <returns></returns>
    Assembly LoadFromName (string name, bool topDirectoryOnly, params string[] directories);

    /// <summary>
    /// Load an assembly given its name and a list of possible directories
    /// </summary>
    /// <param name="name"></param>
    /// <param name="directories"></param>
    /// <param name="topDirectoryOnly"></param>
    /// <returns></returns>
    Assembly LoadFromName (string name, IEnumerable<string> directories, bool topDirectoryOnly = false);

    /// <summary>
    /// Get the plugins directory file repo path (with the '/' separator):
    /// <item>plugins_synchronized (for .NET Framework)</item>
    /// <item>plugins_core (for .NET Core)</item>
    /// </summary>
    /// <returns></returns>
    string GetPluginsFileRepoPath ();
  }

  /// <summary>
  /// Extensions to <see cref="IAssemblyLoader"/>
  /// </summary>
  public static class AssemblyLoaderExtensions
  {
    /// <summary>
    /// Convert the the File repo path into the native OS path
    /// </summary>
    /// <returns></returns>
    public static string GetPluginsFileRepoOsPath (this IAssemblyLoader assemblyLoader)
    {
      return Lemoine.FileRepository.FileRepoPath
        .ConvertToOsPath (assemblyLoader.GetPluginsFileRepoPath ());
    }
  }
}
