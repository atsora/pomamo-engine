// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Abstract class for extensions with multiple instances that are configurable
  /// </summary>
  public abstract class MultipleInstanceConfigurableExtension<TConfiguration>
    : Lemoine.Extensions.IExtension, Lemoine.Extensions.Extension.IConfigurable
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration, new ()
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MultipleInstanceConfigurableExtension<TConfiguration>).FullName);

    #region Members
    IConfigurationLoader<TConfiguration> m_configurationLoader;
    IPluginInstance m_instance = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Configuration context
    /// </summary>
    public IPluginInstance ConfigurationContext
    {
      get { return m_instance; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    protected MultipleInstanceConfigurableExtension (IConfigurationLoader<TConfiguration> configurationLoader)
    {
      m_configurationLoader = configurationLoader;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    protected MultipleInstanceConfigurableExtension ()
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
      get { return false; }
    }
    #endregion // IExtension implementation

    #region IConfigurable implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Extension.IConfigurable"/>
    /// </summary>
    /// <param name="association"></param>
    public void AddConfigurationContext (IPluginInstance association)
    {
      Debug.Assert (null == m_instance); // It must be set only once
      m_instance = association;
    }
    #endregion // IConfigurable implementation

    /// <summary>
    /// Load the configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns>true in case of success</returns>
    protected virtual bool LoadConfiguration (out TConfiguration configuration)
    {
      if (null == m_instance) {
        log.Warn ("LoadConfiguration: association is null, probably because run in a unit test => return the default configuration");
        configuration = new TConfiguration ();
        return true;
      }

      string parameters = m_instance.InstanceParameters;
      try {
        configuration = m_configurationLoader.LoadConfiguration (parameters);
      }
      catch (Exception ex) {
        log.Error ($"LoadConfiguration: parse error for {parameters}, skip this instance", ex);
        configuration = default (TConfiguration);
        return false;
      }

      if (null == configuration) {
        log.WarnFormat ("LoadConfiguration: a null configuration was returned => return false");
        return false;
      }

      IEnumerable<string> errors;
      if (!configuration.IsValid (out errors)) {
        log.Warn ($"LoadConfiguration: the configuration is not valid, skip this instance, errors={errors}");
        return false;
      }

      return true;
    }
  }
}
