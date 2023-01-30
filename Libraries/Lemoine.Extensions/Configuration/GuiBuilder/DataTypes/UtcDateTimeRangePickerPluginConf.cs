// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider a DateTimePicker control to get a date/time
  /// </summary>
  public class UtcDateTimeRangePickerPluginConf : IPluginConfDataType
  {
    ILog log = LogManager.GetLogger (typeof (UtcDateTimeRangePickerPluginConf).FullName);

    /// <summary>
    /// Used
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Not used
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// Not used
    /// </summary>
    public string Parameters
    {
      get; set;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public UtcDateTimeRangePickerPluginConf ()
    {
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    public virtual Action<IPluginConfig, string, string> AddControl
    {
      get
      {
        return AddDateTimeRangePickerControl;
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
    protected void AddDateTimeRangePickerControl (IPluginConfig pluginConfig, string name, string labelText)
    {
      if (this.Optional) {
        pluginConfig.AddOptionalUtcDateTimeRangePicker (name, labelText);
      }
      else {
        pluginConfig.AddUtcDateTimeRangePicker (name, labelText);
      }
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
