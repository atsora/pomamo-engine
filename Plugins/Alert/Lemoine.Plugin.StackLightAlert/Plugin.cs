// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

namespace Lemoine.Plugin.StackLightAlert
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Stack light alert"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Alert by e-mail in case of a stack light change";
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

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);
  }
}
