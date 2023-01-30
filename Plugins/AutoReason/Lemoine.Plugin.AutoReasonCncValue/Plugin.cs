// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Plugin.AutoReasonCncValue
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : Extensions.AutoReason.AutoReasonAutoConfigNoDefaultReasonPlugin<Configuration>, IPluginDll
  {
    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Auto-reason cnc value"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Trigger an auto-reason that is based on a cnc value";
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
    /// 
    /// Do not create any default auto-reason here
    /// </summary>
    public Plugin ()
      : base ()
    { }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
