// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Extension that will be executed at a specific location within the system
  /// </summary>
  public interface IExtension
  {
    /// <summary>
    /// Use a unique instance for this extension, else multiple instances are used,
    /// one per configuration
    /// </summary>
    bool UniqueInstance { get; }
  }

  /// <summary>
  /// Extensions to the interface IExtension
  /// </summary>
  public static class ExtensionExtensions
  {
    /// <summary>
    /// Get the plugin name (part in the assembly after Lemoine.Plugin.)
    /// </summary>
    /// <returns></returns>
    public static string GetPluginName (this IExtension extension)
    {
      var assemblyQualifiedName = extension.GetType ().AssemblyQualifiedName;
      var pluginName = assemblyQualifiedName.Replace ("Lemoine.Plugin.", "");
      return pluginName;
    }
  }
}
