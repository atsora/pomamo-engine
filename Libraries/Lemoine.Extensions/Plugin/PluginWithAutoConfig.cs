// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public abstract class PluginWithAutoConfig<TConfiguration> : PluginWithConfig<TConfiguration>
    where TConfiguration : IConfiguration, new()
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginWithAutoConfig<TConfiguration>).FullName);

    IPluginConfigurationControl m_configurationControl = null;

    #region Constructors
    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public PluginWithAutoConfig ()
      : base ()
    {
    }

    /// <summary>
    /// Constructor with a configuration loader
    /// </summary>
    /// <param name="configurationLoader"></param>
    public PluginWithAutoConfig (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
    {
    }
    #endregion // Constructors

    #region IPlugin interface
    /// <summary>
    /// Configuration interface of the plugin
    /// Modified after the installation
    /// May be null
    /// </summary>
    public override IPluginConfigurationControl ConfigurationControl
    {
      get
      {
        if (null == m_configurationControl) {
          m_configurationControl = new Lemoine.Extensions.Configuration.GuiBuilder.AutoConfigGuiBuilder<TConfiguration> ();
        }
        return m_configurationControl;
      }
    }
    #endregion // IPlugin interface
  }
}
