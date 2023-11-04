// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.AcquisitionDelayEvent
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "Acquisition delay event";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Create an event that can be used to send e-mail in case of acquisition delay";

    public PluginFlag Flags => PluginFlag.Event | PluginFlag.Analysis;

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
