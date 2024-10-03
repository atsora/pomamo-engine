// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copytight (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using System.Data;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.ReportingConfig
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "ReportingConfig";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Plugin to update a configuration in ReportWebApp";

    public PluginFlag Flags => PluginFlag.Config;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 2;

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);
  }
}
