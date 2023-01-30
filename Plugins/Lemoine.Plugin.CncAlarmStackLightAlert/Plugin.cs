// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.CncAlarmStackLightAlert
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Cnc stacklight alarm alert"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description {
      get {
        return "Alert by e-mail in case of a cnc alarm occurring during a stacklight event";
      }
    }

    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.Alert;
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    #endregion // Getters / Setters
  }
}
