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
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public abstract class PluginWithAutoConfigCustomAction<TConfiguration, TCustomAction> : PluginWithAutoConfig<TConfiguration>
    where TConfiguration : IConfiguration, new()
    where TCustomAction : ICustomAction<TConfiguration>, IConfiguration, new()
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginWithAutoConfigCustomAction<TConfiguration, TCustomAction>).FullName);

    IPluginCustomActionControl m_customActionControl = null;

    #region Constructors
    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public PluginWithAutoConfigCustomAction ()
      : base ()
    {
    }

    /// <summary>
    /// Constructor with a configuration loader
    /// </summary>
    /// <param name="configurationLoader"></param>
    public PluginWithAutoConfigCustomAction (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
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
          m_customActionControl = new AutoCustomActionGuiBuilder<TCustomAction, TConfiguration> ();
        }
        return new List<IPluginCustomActionControl> { m_customActionControl };
      }
    }
    #endregion // IPlugin interface
  }
}
