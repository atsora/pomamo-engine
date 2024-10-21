// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.CncAlarmSequenceAlert
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "Cnc alarm sequence alert";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Alert by e-mail in case of a cnc alarm change with the corresponding sequence";

    public PluginFlag Flags => PluginFlag.Alert;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;
  }
}
