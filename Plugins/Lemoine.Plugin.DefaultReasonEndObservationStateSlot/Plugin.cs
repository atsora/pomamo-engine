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

namespace Lemoine.Plugin.DefaultReasonEndObservationStateSlot
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin: PluginWithConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    #region Members
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Default reason at the end of an observation state slot"; } }
    
    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description {
      get {
        return "Override the default reason for periods at the end of an observation state slot.";
      }
    }

    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.Analysis | PluginFlag.Web;
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    
    /// <summary>
    /// Configuration interface of the plugin
    /// May be null
    /// </summary>
    public override IPluginConfigurationControl ConfigurationControl { get { return null; } }    
    #endregion // Getters / Setters
    
    static readonly ILog log = LogManager.GetLogger(typeof (Plugin).FullName);

  }
}
