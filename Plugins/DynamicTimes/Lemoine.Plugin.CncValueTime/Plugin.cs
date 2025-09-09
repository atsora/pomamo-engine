// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Pulse.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Plugin.CncValueTime
{
  /// <summary>
  /// Trigger a production start of n good cycles
  /// </summary>
  public class Plugin : PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "CncValueTime";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Find the time when a cnc value meets a given condition";

    public PluginFlag Flags => PluginFlag.Analysis | PluginFlag.AutoReason;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;
  }
}
