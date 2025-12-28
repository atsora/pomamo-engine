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

namespace Lemoine.Plugin.ReasonToProductionState
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
    public override string Name => "ReasonToProductionState";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Determine a production state from a reason";

    public PluginFlag Flags => PluginFlag.Analysis;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;

    #region Methods

    #endregion // Methods
  }
}
