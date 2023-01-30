// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.AutoReason;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Plugin.AutoReasonForward
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin
    : AutoReasonAutoConfigNoDefaultReasonPlugin<Configuration>, IPluginDll
  {
    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Auto-reason forward"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Auto-reason plugin to forward an auto-reason to another machine. The score is taken from the configuration.";
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin ()
    { }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
