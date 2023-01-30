// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Package
{
  /// <summary>
  /// Plugin description
  /// </summary>
  public class PluginDescription
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PluginDescription).FullName);

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, name of the assembly without Lemoine.Plugin and without .dll
    /// 
    /// For example: ShortPeriodRemoval for Lemoine.Plugin.ShortPeriodRemoval.dll
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Configurations / Instances
    /// </summary>
    public IList<PluginInstance> Instances { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PluginDescription ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
