// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider a set of database items
  /// </summary>
  public abstract class DataPluginConf<T, TConfigurationKey> : IPluginConfDataType
    where T: IDisplayable
  {
    #region Members
    IEnumerable<T> m_items = null;
    #endregion // Members

    /// <summary>
    /// Not used
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Allow multiple values
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// Not used
    /// </summary>
    public string Parameters { get; set; }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    public virtual Action<IPluginConfig, string, string> AddControl
    {
      get
      {
        return ConfigureControl;
      }
    }

    /// <summary>
    /// To override: get all the possible items
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<T> FindAll ();

    /// <summary>
    /// To override: get the configuration key from a data item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected abstract TConfigurationKey GetConfigurationKey (T item);

    /// <summary>
    /// To override: does an item match a configuration key
    /// </summary>
    /// <param name="item"></param>
    /// <param name="configurationKey"></param>
    /// <returns></returns>
    protected abstract bool TestConfigurationKey (T item, TConfigurationKey configurationKey);

    void Load ()
    {
      if (null == m_items) {
        using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_items = FindAll ();
        }
      }
    }

    IEnumerable<object> GetItems ()
    {
      Load ();
      return m_items.Cast<object> ();
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public virtual Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return c => GetProperty (property, c);
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public virtual Action<TConfiguration, object> GetConfigurationSetter<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return delegate (TConfiguration c, object v)
      { SetProperty (property, c, v); };
    }

    /// <summary>
    /// Add a text control
    /// </summary>
    /// <param name="pluginConfig"></param>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    protected void ConfigureControl (IPluginConfig pluginConfig, string name, string labelText)
    {
      if (this.Multiple) { // Multiple => ListBox
        AddListBox (pluginConfig, name, labelText);
      }
      else { // Single => ComboBox
        AddComboBox (pluginConfig, name, labelText);
      }
    }

    /// <summary>
    /// Add a combo box
    /// </summary>
    /// <param name="pluginConfig"></param>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    void AddComboBox (IPluginConfig pluginConfig, string name, string labelText)
    {
      pluginConfig.AddComboBox (name, labelText, GetItems, "Display", this.Optional);
    }

    /// <summary>
    /// Add a list box
    /// </summary>
    /// <param name="pluginConfig"></param>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    void AddListBox (IPluginConfig pluginConfig, string name, string labelText)
    {
      pluginConfig.AddListBox<T> (name, labelText, GetItems, "Display", this.Optional, this.Multiple);
    }

    /// <summary>
    /// Get the property of the configuration
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="propertyInfo"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected virtual object GetProperty<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      Load ();
      if (this.Multiple) {
        var configurationKeys = (IEnumerable<TConfigurationKey>)propertyInfo.GetValue (configuration, null);
        if (null == configurationKeys) {
          return new List<T> ();
        }
        else {
          return m_items.Where (m => configurationKeys.Any (x => TestConfigurationKey (m, x)));
        }
      }
      else {
        var configurationKey = (TConfigurationKey)propertyInfo.GetValue (configuration, null);
        return m_items.FirstOrDefault (m => TestConfigurationKey (m, configurationKey));
      }
    }

    /// <summary>
    /// Set the property of the configuration
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="propertyInfo"></param>
    /// <param name="configuration"></param>
    /// <param name="v"></param>
    protected virtual void SetProperty<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      if (this.Multiple) {
        IEnumerable<TConfigurationKey> configurationKeys;
        if (null == v) {
          configurationKeys = new List<TConfigurationKey> ();
        }
        else if (v is IEnumerable<T>) {
          configurationKeys = ((IEnumerable<T>)v)
            .Cast<T> ()
            .Select (x => GetConfigurationKey (x));
        }
        else {
          configurationKeys = new List<TConfigurationKey> { GetConfigurationKey ((T)v) };
        }
        propertyInfo.SetValue (configuration, configurationKeys.ToList (), null);
      }
      else { // !this.Multiple
        if (null == v) {
          propertyInfo.SetValue (configuration, 0, null);
        }
        else {
          var item = (T)v;
          propertyInfo.SetValue (configuration, GetConfigurationKey (item), null);
        }
      }
    }
  }
}
