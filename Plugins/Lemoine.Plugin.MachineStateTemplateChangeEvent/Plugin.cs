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

namespace Lemoine.Plugin.MachineStateTemplateChangeEvent
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
    public override string Name { get { return "Machine state template change event"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get {
        return "Create an event that can be used to send e-mail when a machine state template changes";
      }
    }

    public PluginFlag Flags
    {
      get {
        return PluginFlag.Event | PluginFlag.Analysis;
      }
    }

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => true;

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
