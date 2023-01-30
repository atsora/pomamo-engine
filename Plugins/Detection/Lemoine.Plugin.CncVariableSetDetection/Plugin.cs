// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.CncVariableSetDetection
{
  /// <summary>
  /// Interpret both a stamp a stamp and a milestone
  /// </summary>
  public class Plugin : PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "Cnc variable set detection";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Plugin to use the cnc variable set to detect stamps and milestones";

    public PluginFlag Flags => PluginFlag.Cnc | PluginFlag.Analysis | PluginFlag.AutoReason | PluginFlag.Web;

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => true;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);
  }
}
