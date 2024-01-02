// Copyright (C) 2023 Atsora Solutions

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Plugin load status
  /// </summary>
  public enum PluginLoadStatus
  {
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown,
    /// <summary>
    /// Plugin successfully loaded
    /// </summary>
    Success,
    /// <summary>
    /// The flags don't match and the plugin was not loaded
    /// </summary>
    NotMatchingFlags,
    /// <summary>
    /// The IPluginDll class is missing
    /// </summary>
    MissingPluginClass,
    /// <summary>
    /// The assembly was not found
    /// </summary>
    AssemblyNotFound,
    /// <summary>
    /// The assembly could not be loaded
    /// </summary>
    AssemblyLoadException,
  }

  /// <summary>
  /// Description of ExtensionLoader.
  /// </summary>
  public interface IPluginDllLoader
  {
    /// <summary>
    /// Plugin load status
    /// </summary>
    PluginLoadStatus Status { get; }

    /// <summary>
    /// Plugin name
    /// </summary>
    string PluginName { get; }

    /// <summary>
    /// Return true if the dll is valid
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Plugin loaded
    /// </summary>
    IPluginDll Plugin { get; }

    /// <summary>
    /// Associated assembly
    /// </summary>
    Assembly Assembly { get; }

    /// <summary>
    /// Did the filter match the plugin ?
    /// </summary>
    bool FilterMatch { get; }

    /// <summary>
    /// Associated extensions
    /// </summary>
    IList<IExtension> Extensions { get; }
  }
}
