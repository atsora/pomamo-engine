// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Interfaces
{

  /// <summary>
  /// Interface that should implement every dll to be loaded
  /// </summary>
  public interface IPluginDll : IEquatable<IPluginDll>
  {
    /// <summary>
    /// Identifying name, based on the assembly name
    /// </summary>
    string IdentifyingName { get; }

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Can the plugin have multiple configurations for the same package?
    /// </summary>
    bool MultipleConfigurations { get; }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Associated extensions
    /// </summary>
    IEnumerable<Type> ActiveExtensionTypes { get; set; }
    
    /// <summary>
    /// Configuration interface of the plugin
    /// Modified after the installation
    /// May be null
    /// </summary>
    IPluginConfigurationControl ConfigurationControl { get; }

    /// <summary>
    /// List of custom actions
    /// May be null
    /// </summary>
    IList<IPluginCustomActionControl> CustomActionControls { get; }

    /// <summary>
    /// Context of the plugin
    /// Set by the dll loader and extension manager
    /// </summary>
    IPluginContext Context { get; set; }

    /// <summary>
    /// Instances that are associated to the plugin
    /// </summary>
    IEnumerable<IPluginInstance> Instances { get; }

    /// <summary>
    /// Add an instance
    /// </summary>
    /// <param name="instance">not null</param>
    void AddInstance (IPluginInstance instance);

    /// <summary>
    /// Install from a specific version
    /// (create tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    /// <param name="baseVersion">last version already installed</param>
    void Install (int baseVersion);

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    void Uninstall ();

    /// <summary>
    /// Check the consistency of the properties for the plugin to run
    /// </summary>
    /// <param name="configurationText"></param>
    /// <returns>Can be null or empty if there is no errors</returns>
    IEnumerable<string> GetConfigurationErrors (string configurationText);

    /// <summary>
    /// Get the configuration errors based on the loading instances
    /// </summary>
    /// <returns>Can be null or empty it there are no errors</returns>
    IEnumerable<string> GetConfigurationErrors ();

    /// <summary>
    /// Get the configuration errors for a specific package based on the loading instances
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns>Can be null or empty it there are no errors</returns>
    IEnumerable<string> GetConfigurationErrorsForSpecificPackage (string packageName);
  }
}
