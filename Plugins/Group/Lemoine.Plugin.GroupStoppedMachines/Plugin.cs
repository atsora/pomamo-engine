// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using System.Data;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.GroupStoppedMachines
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
    public override string Name { get { return "GroupStoppedMachines"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Add a group for the stopped machines";
      }
    }

    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.Web;
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }

    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Methods

    #endregion // Methods
  }
}
