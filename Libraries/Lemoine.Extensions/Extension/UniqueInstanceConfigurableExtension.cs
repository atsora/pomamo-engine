// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Newtonsoft.Json;
using Lemoine.Core.Log;
using Lemoine.Conversion.JavaScript;
using Lemoine.Extensions;
using Lemoine.Extensions.Configuration;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Abstract class for extensions with a unique instance that are configurable
  /// </summary>
  public abstract class UniqueInstanceConfigurableExtension<TConfiguration>
    : Lemoine.Extensions.IExtension, Lemoine.Extensions.Extension.IConfigurable
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UniqueInstanceConfigurableExtension<TConfiguration>).FullName);

    #region Members
    IConfigurationLoader<TConfiguration> m_configurationLoader;
    readonly IList<IPluginInstance> m_instances = new List<IPluginInstance> ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Configuration context
    /// </summary>
    public IEnumerable<IPluginInstance> ConfigurationContexts
    {
      get { return m_instances; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    protected UniqueInstanceConfigurableExtension (IConfigurationLoader<TConfiguration> configurationLoader)
    {
      m_configurationLoader = configurationLoader;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    protected UniqueInstanceConfigurableExtension ()
      : this (new Lemoine.Extensions.Configuration.ConfigurationLoader<TConfiguration> ())
    {
    }
    #endregion // Constructors

    #region IExtension implementation
    /// <summary>
    /// <see cref="IExtension"/>
    /// </summary>
    public bool UniqueInstance
    {
      get { return true; }
    }
    #endregion // IExtension implementation

    #region IConfigurableExtension implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Extension.IConfigurable"/>
    /// </summary>
    /// <param name="instance"></param>
    public void AddConfigurationContext (IPluginInstance instance)
    {
      m_instances.Add (instance);
    }

    #endregion // IConfigurableExtension implementation

    /// <summary>
    /// Load the configurations
    /// </summary>
    protected virtual IEnumerable<TConfiguration> LoadConfigurations ()
    {
      IList<TConfiguration> configurations = new List<TConfiguration> ();

      foreach (var association in m_instances) {
        string parameters = association.InstanceParameters;
        TConfiguration configuration;
        try {
          configuration = m_configurationLoader.LoadConfiguration (parameters);
          IEnumerable<string> errors;
          if (!configuration.IsValid (out errors)) {
            log.Warn ("LoadConfiguration: the configuration is not valid, skip this instance");
          }
          else {
            configurations.Add (configuration);
          }
        }
        catch (Exception ex) {
          log.Error ($"LoadConfiguration: parse error for {parameters}, skip this instance", ex);
        }
      }

      return configurations;
    }
  }
}
