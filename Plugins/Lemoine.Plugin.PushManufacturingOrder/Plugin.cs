// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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

namespace Lemoine.Plugin.PushManufacturingOrder
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "Push manufacturing order";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Plugin to add a new web service to push a manufacturing order into the system";

    public PluginFlag Flags => PluginFlag.Web;

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => false;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);
  }
}
