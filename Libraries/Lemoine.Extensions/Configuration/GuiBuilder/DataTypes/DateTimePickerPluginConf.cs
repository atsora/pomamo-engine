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
  public class DateTimePickerPluginConf : IPluginConfDataType
  {
    ILog log = LogManager.GetLogger (typeof (DateTimePickerPluginConf).FullName);

    /// <summary>
    /// Used
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Not used
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// Set a positive number of seconds to activate their display of seconds
    /// </summary>
    public string Parameters
    {
      get
      {
        return TimeSpan.FromSeconds (this.WithSeconds ? 1 : 0).ToString ();
      }
      set
      {
        if (string.IsNullOrEmpty (value)) { // Default
          this.WithSeconds = true;
        }
        else {
          try {
            var timeSpan = TimeSpan.Parse (value);
            this.WithSeconds = timeSpan.Seconds != 0;
          }
          catch (Exception ex) {
            log.ErrorFormat ("Parameters.set: invalid value {0}", value);
            log.Error ("Parameters.set: exception", ex);
          }
        }
      }
    }

    /// <summary>
    /// Use seconds (true by default)
    /// </summary>
    public bool WithSeconds { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public DateTimePickerPluginConf ()
    {
      this.WithSeconds = true;
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    public virtual Action<IPluginConfig, string, string> AddControl
    {
      get
      {
        return AddDateTimePickerControl;
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
    protected void AddDateTimePickerControl (IPluginConfig pluginConfig, string name, string labelText)
    {
      if (this.Optional) {
        pluginConfig.AddOptionalDateTimePicker (name, labelText, this.WithSeconds);
      }
      else {
        pluginConfig.AddDateTimePicker (name, labelText, this.WithSeconds);
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
