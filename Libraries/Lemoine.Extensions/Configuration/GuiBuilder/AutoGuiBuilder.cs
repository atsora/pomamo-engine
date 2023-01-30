// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Plugin;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lemoine.Extensions.Configuration.GuiBuilder
{
  /// <summary>
  /// Item for the ConfigurationGuiBuilder
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public class ConfigGuiItem<TConfiguration>
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration
  {
    /// <summary>
    /// Control name prefix
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// Control label text
    /// </summary>
    public string LabelText { get; private set; }
    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; private set; }
    /// <summary>
    /// Action to add the control
    /// </summary>
    public Action<IPluginConfig, string, string> AddControl { get; private set; }
    /// <summary>
    /// Function to read the value from the configuration
    /// </summary>
    public Func<TConfiguration, object> ConfigurationReader { get; private set; }
    /// <summary>
    /// Action to set the new value into the configuration
    /// </summary>
    public Action<TConfiguration, object> ConfigurationSetter { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="description"></param>
    /// <param name="addControl"></param>
    /// <param name="configurationReader"></param>
    /// <param name="configurationSetter"></param>
    public ConfigGuiItem (string name, string labelText, string description,
      Action<IPluginConfig, string, string> addControl,
      Func<TConfiguration, object> configurationReader,
      Action<TConfiguration, object> configurationSetter)
    {
      this.Name = name;
      this.LabelText = labelText;
      this.Description = description;
      this.AddControl = addControl;
      this.ConfigurationReader = configurationReader;
      this.ConfigurationSetter = configurationSetter;
    }
  }

  /// <summary>
  /// Base class to make a new GuiBuilder
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class AutoGuiBuilder<T>
    where T : Lemoine.Extensions.Configuration.IConfiguration, new()
  {
    #region Members
    IPluginConfig m_pluginConfig;
    readonly IList<ConfigGuiItem<T>> m_items = new List<ConfigGuiItem<T>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (AutoGuiBuilder<T>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoGuiBuilder ()
    {
      var configurationType = typeof (T);
      var properties = configurationType.GetProperties ();
      foreach (var property in properties) {
        var attribute = Attribute.GetCustomAttributes (property)
          .FirstOrDefault (a => a is PluginConfAttribute);
        if (null != attribute) {
          var pluginConfAttribute = (PluginConfAttribute)attribute;
          string name = property.Name;
          var currentProperty = property;
          var dataType = pluginConfAttribute.DataType;
          AddItem (name,
            pluginConfAttribute.Label,
            pluginConfAttribute.Description,
            dataType.AddControl,
            dataType.GetConfigurationReader<T> (currentProperty),
            dataType.GetConfigurationSetter<T> (currentProperty));
        }
      }
    }

    /// <summary>
    /// Add an item
    /// </summary>
    /// <param name="item"></param>
    protected void AddItem (ConfigGuiItem<T> item)
    {
      m_items.Add (item);
    }

    /// <summary>
    /// Add an item
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="description"></param>
    /// <param name="addControl"></param>
    /// <param name="configurationReader"></param>
    /// <param name="configurationSetter"></param>
    protected void AddItem (string name, string labelText, string description,
      Action<IPluginConfig, string, string> addControl,
      Func<T, object> configurationReader,
      Action<T, object> configurationSetter)
    {
      AddItem (new ConfigGuiItem<T> (name, labelText, description, addControl, configurationReader, configurationSetter));
    }

    /// <summary>
    /// Create the configuration (to override)
    /// </summary>
    /// <returns></returns>
    virtual protected T CreateConfiguration ()
    {
      return new T ();
    }

    /// <summary>
    /// Add a text control
    /// </summary>
    /// <param name="pluginConfig"></param>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    protected void AddTextControl (IPluginConfig pluginConfig, string name, string labelText)
    {
      pluginConfig.AddText (name, labelText);
    }

    #region IConfigurationGuiBuilder implementation
    /// <summary>
    /// <see cref="IConfigurationGuiBuilder"/>
    /// </summary>
    /// <param name="pluginConfig"></param>
    public void SetConfigControl (IPluginConfig pluginConfig)
    {
      m_pluginConfig = pluginConfig;
    }
    #endregion // IConfigurationGuiBuilder implementation

    #region IPluginConfigurationControl implementation
    /// <summary>
    /// <see cref="IPluginConfigurationControl"/>
    /// </summary>
    public virtual string Help
    {
      get
      {
        string help = "Select:";
        foreach (var item in m_items) {
          help += "\n- ";
          help += item.Description;
        }
        return help;
      }
    }

    /// <summary>
    /// <see cref="IPluginConfigurationControl"/>
    /// </summary>
    public void InitializeInterface ()
    {
      m_pluginConfig.BeforeAddingControls ();
      foreach (var item in m_items) {
        item.AddControl (m_pluginConfig, item.Name, item.LabelText);
      }
      m_pluginConfig.AfterAddingControls ();
    }

    /// <summary>
    /// <see cref="IPluginConfigurationControl"/>
    /// </summary>
    /// <returns></returns>
    public IList<string> GetErrors ()
    {
      var configuration = GetConfiguration ();
      if (!configuration.IsValid (out var errors)) {
        return errors.ToList ();
      }
      else {
        return null;
      }
    }
    #endregion // IPluginConfigurationControl implementation

    /// <summary>
    /// Get the associated configuration
    /// </summary>
    /// <returns></returns>
    protected T GetConfiguration ()
    {
      var configuration = CreateConfiguration ();
      foreach (var item in m_items) {
        item.ConfigurationSetter (configuration, m_pluginConfig.Get (item.Name));
      }
      return configuration;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    protected void LoadProperties (T configuration)
    {
      m_pluginConfig.InitialLoad ();

      foreach (var item in m_items) {
        m_pluginConfig.Set (item.Name, item.ConfigurationReader (configuration));
      }
    }

  }

  /// <summary>
  /// Base class to make a new GuiBuilder with a configuration loader
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="TConfiguration">type for the configuration loader</typeparam>
  public class AutoGuiBuilderWithConfigurationLoader<T, TConfiguration>
    : AutoGuiBuilder<T>
    where T : Lemoine.Extensions.Configuration.IConfiguration, new()
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration, new()
  {
    #region Members
    readonly ConfigurationLoader<TConfiguration> m_configurationLoader;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (AutoGuiBuilderWithConfigurationLoader<T, TConfiguration>).FullName);

    /// <summary>
    /// Associated configuration loader
    /// </summary>
    protected ConfigurationLoader<TConfiguration> ConfigurationLoader => m_configurationLoader;

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoGuiBuilderWithConfigurationLoader ()
      : this (new ConfigurationLoader<TConfiguration> ())
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    public AutoGuiBuilderWithConfigurationLoader (ConfigurationLoader<TConfiguration> configurationLoader)
    {
      m_configurationLoader = configurationLoader;
    }
  }
}

