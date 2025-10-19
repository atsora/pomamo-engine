// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.SetupSwitcher
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin: PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin () : base (new ConfigurationLoader ())
    {
    }

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "Setup switcher";
    
    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Trigger a switch to a new machine state template when a new operation is detected";

    public PluginFlag Flags => PluginFlag.Analysis;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 3;
    
    #region Methods
    #endregion // Methods
  }
}
