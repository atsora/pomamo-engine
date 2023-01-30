// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.AllCyclesFull
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin: PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    #region Members
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "All Cycles Full"; } }
    
    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description {
      get {
        return "Turn all cycles to be considered as full cycles";
      }
    }
    
    /// <summary>
    /// <see cref="IPluginDll"/>
    /// </summary>
    public PluginFlag Flags {
      get { return PluginFlag.Analysis; }
    }

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => false;
    
    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    #endregion // Getters / Setters
    
    static readonly ILog log = LogManager.GetLogger(typeof (Plugin).FullName);
    
    #region Methods
    #endregion // Methods
  }
}
