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

namespace Lemoine.Plugin.SameCncValue
{
  /// <summary>
  /// Dynamic time on cnc values
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "SameCncValue";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Get a dynamic period corresponding to a same and consecutive cnc value";

    public PluginFlag Flags => PluginFlag.Analysis | PluginFlag.AutoReason;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;
  }
}
