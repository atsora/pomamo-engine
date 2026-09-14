// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Pulse.Extensions.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.Plugin.AutoMachineStateTemplateConfig
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "AutoMachineStateTemplateConfig";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Auto-machine state template from machine activity using the automachinestatetemplate table";

    public PluginFlag Flags => PluginFlag.Analysis;

    public override bool MultipleConfigurations => false;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;
  }
}
