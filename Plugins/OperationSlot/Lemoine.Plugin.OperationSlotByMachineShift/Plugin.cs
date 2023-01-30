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

namespace Lemoine.Plugin.OperationSlotByMachineShift
{
  /// <summary>
  /// Plugin
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin ()
    {
    }

    public override string Name => "OperationSlotByMachineShift";

    public override string Description => "Operation slot is split by machine shift (observation state slot shift)";

    public PluginFlag Flags => PluginFlag.Analysis | PluginFlag.Web;

    public override int Version => 1;
    #endregion // Constructors

  }
}
