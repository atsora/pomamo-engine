// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.ProductionTracker
{
  /// <summary>
  /// Plugin
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "ProductionTracker";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Additional web services for the production tracker web page. Note that it depends on plugin HourlyIntermediateWorkPieceSummary";

    public PluginFlag Flags => PluginFlag.Web;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;
  }
}
