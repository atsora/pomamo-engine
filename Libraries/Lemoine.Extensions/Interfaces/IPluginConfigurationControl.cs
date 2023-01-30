// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Interface for a plugin configuration control
  /// </summary>
  public interface IPluginConfigurationControl
  {
    #region Getters / Setters
    /// <summary>
    /// Help text for the configuration
    /// </summary>
    string Help { get; }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Initialize the interface
    /// </summary>
    void InitializeInterface ();

    /// <summary>
    /// Load the properties into the interface
    /// </summary>
    /// <param name="configurationText">can be null</param>
    void LoadProperties (string configurationText);

    /// <summary>
    /// Save the properties from the interface
    /// </summary>
    /// <returns></returns>
    string GetProperties ();

    /// <summary>
    /// Get the configuration errors
    /// </summary>
    /// <returns></returns>
    IList<string> GetErrors ();
    #endregion // Methods
  }
}
