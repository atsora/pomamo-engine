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
using Pulse.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Plugin.NoResetShortGapStoppedCncValue
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "No reset after a short gap of a stopped cnc value"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Plugin not to stop the cnc values that are flagged as 'Stopped' after a short gap";
      }
    }

    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.CncData;
      }
    }

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations
    {
      get { return true; }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    protected override void InstallVersion (int version)
    {
      // Nothing to do
    }

    #region Methods
    #endregion // Methods
  }
}
