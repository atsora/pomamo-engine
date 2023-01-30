// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration;
using Lemoine.Extensions.CustomAction;
using Lemoine.Extensions.Configuration.Implementation;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// Plugin with an auto custom action but no config
  /// </summary>
  public abstract class PluginWithAutoCustomAction<TCustomAction> : PluginNoConfig
      where TCustomAction : ICustomAction<EmptyConfiguration>, IConfiguration, new()
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginWithAutoCustomAction<TCustomAction>).FullName);

    IPluginCustomActionControl m_customActionControl = null;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public PluginWithAutoCustomAction ()
      : base ()
    {
    }
    #endregion // Constructors

    #region IPlugin interface
    /// <summary>
    /// List of custom actions, may be null
    /// </summary>
    public override IList<IPluginCustomActionControl> CustomActionControls
    {
      get
      {
        if (null == m_customActionControl) {
          m_customActionControl = new AutoCustomActionGuiBuilder<TCustomAction, EmptyConfiguration> ();
        }
        return new List<IPluginCustomActionControl> { m_customActionControl };
      }
    }
    #endregion // IPlugin interface
  }
}
