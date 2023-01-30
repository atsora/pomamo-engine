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
  public abstract class PluginWithConfig<TConfiguration> : GenericPluginDll
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginWithConfig<TConfiguration>).FullName);

    IConfigurationLoader<TConfiguration> m_configurationLoader;

    /// <summary>
    /// Configuration interface of the plugin
    /// Modified after the installation
    /// May be null
    /// </summary>
    public abstract override IPluginConfigurationControl ConfigurationControl { get; }

    /// <summary>
    /// <see cref="GenericPluginDll"/>
    /// </summary>
    public override bool MultipleConfigurations => true;

    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public PluginWithConfig ()
    {
      m_configurationLoader = new ConfigurationLoader<TConfiguration> ();
    }

    /// <summary>
    /// Constructor with a configuration loader
    /// </summary>
    /// <param name="configurationLoader"></param>
    public PluginWithConfig (IConfigurationLoader<TConfiguration> configurationLoader)
    {
      m_configurationLoader = configurationLoader;
    }

    /// <summary>
    /// Check the consistency of the properties for the plugin to run
    /// </summary>
    /// <param name="configurationText"></param>
    /// <returns>Can be null or empty if there are no errors</returns>
    public override IEnumerable<string> GetConfigurationErrors (string configurationText)
    {
      TConfiguration configuration;
      try {
        configuration = m_configurationLoader.LoadConfiguration (configurationText);
      }
      catch (Exception ex) {
        log.Error ($"GetConfigurationErrors: parse error in {configurationText}", ex);
        IList<string> parsingErrors = new List<string> ();
        parsingErrors.Add ("Parse error of the configuration string");
        return parsingErrors;
      }

      if (null == configuration) {
        if (log.IsErrorEnabled) {
          log.Error ("GetConfigurationErrors: a null configuration was returned");
        }
        IList<string> parsingErrors = new List<string> ();
        parsingErrors.Add ("Configuration is null (empty parameters ?)");
        return parsingErrors;
      }

      {
        IEnumerable<string> configurationErrors;
        if (!configuration.IsValid (out configurationErrors)) {
          return configurationErrors;
        }
      }

      return null;
    }
  }
}
