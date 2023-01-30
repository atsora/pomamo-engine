// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.ReasonDefaultManagement
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin
    : Lemoine.Extensions.Plugin.PluginNoConfig
    , IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin ()
    {
    }

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "ReasonDefaultManagement";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Reason management: default behavior";

    public PluginFlag Flags => PluginFlag.Analysis | PluginFlag.Web;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 0;
  }
}
