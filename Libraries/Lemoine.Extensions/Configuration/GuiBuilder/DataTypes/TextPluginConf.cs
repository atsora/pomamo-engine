// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider a TextBox control will be used to get a string
  /// </summary>
  public class TextPluginConf : IPluginConfDataType
  {
    /// <summary>
    /// Not used
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Not used
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
        return AddTextControl;
      }
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
    protected void AddTextControl (IPluginConfig pluginConfig, string name, string labelText)
    {
      pluginConfig.AddText (name, labelText);
    }

    /// <summary>
    /// Get the property of the configuration
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="propertyInfo"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected object GetProperty<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      return propertyInfo.GetValue (configuration, null);
    }

    /// <summary>
    /// Set the property of the configuration
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="propertyInfo"></param>
    /// <param name="configuration"></param>
    /// <param name="v"></param>
    protected void SetProperty<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      propertyInfo.SetValue (configuration, v, null);
    }
  }
}
