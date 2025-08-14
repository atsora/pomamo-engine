// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

namespace Lemoine.Plugin.DefaultCycleCounter
{
  /// <summary>
  /// Plugin
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin ()
    {
    }

    public override string Name => "DefaultCycleCounter";

    public override string Description => "Default cycle counter";

    public PluginFlag Flags => PluginFlag.Analysis | PluginFlag.Web;

    public override int Version => 2;

  }
}
