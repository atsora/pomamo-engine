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

namespace Lemoine.Plugin.SetupSwitcher
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin: PluginWithAutoConfig<Configuration>, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Members
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin () : base (new ConfigurationLoader ())
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Setup switcher"; } }
    
    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description {
      get {
        return "Trigger a switch to a new machine state template when a new operation is detected";
      }
    }

    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.Analysis;
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 3; } }
    #endregion // Getters / Setters
    
    #region Methods
    #endregion // Methods
  }
}
