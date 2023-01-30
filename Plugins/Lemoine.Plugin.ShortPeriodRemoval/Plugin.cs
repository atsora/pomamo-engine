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

namespace Lemoine.Plugin.ShortPeriodRemoval
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
    public override string Name { get { return "Short inactivity removal"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get {
        return "Remove the short periods of inactivity";
      }
    }

    public PluginFlag Flags
    {
      get {
        return PluginFlag.Analysis;
      }
    }

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => true;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 4; } }
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Methods
    #endregion // Methods
  }
}
