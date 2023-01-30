// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.DefaultReasonMinimalConfig
{
  /// <summary>
  /// Plugin to set default reasons with no config using default settings
  /// </summary>
  public class Plugin : PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "DefaultReasonMinimalConfig";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Set default reasons with no custom configuration using default and minimal settings";

    public PluginFlag Flags => PluginFlag.Analysis | PluginFlag.Web;

    public override bool MultipleConfigurations => false;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;
  }
}
