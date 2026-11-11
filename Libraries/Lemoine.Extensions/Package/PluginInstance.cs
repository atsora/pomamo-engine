// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Extensions.Package
{
  /// <summary>
  /// Description of a plugin configuration
  /// </summary>
  public class PluginInstance
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginInstance).FullName);

    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Configuration
    /// </summary>
    public object Parameters { get; set; }

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PluginInstance ()
    {
    }
    #endregion // Constructors
  }
}
