// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Business.Config
{
  /// <summary>
  /// Extension to define actions to take when a plugin is installed or removed
  /// or when a part of the installation process is updated (for example when PulseWebApp is installed)
  /// </summary>
  public interface IInstallationExtension : IExtension
  {
    /// <summary>
    /// CheckConfig is called with an ascending priority order,
    /// RemoveConfig is called with a descending priority order
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// Check a configuration that is related to the plugin is correctly set
    /// </summary>
    /// <returns>Success</returns>
    bool CheckConfig ();

    /// <summary>
    /// Remove a config because the package is uninstalled or disabled
    /// </summary>
    /// <returns>Success</returns>
    bool RemoveConfig ();
  }
}
