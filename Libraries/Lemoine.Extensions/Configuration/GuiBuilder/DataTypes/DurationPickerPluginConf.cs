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
  /// Plugin conf to consider a DurationPicker control to get a time span
  /// </summary>
  public class DurationPickerPluginConf : IPluginConfDataType
  {
    ILog log = LogManager.GetLogger (typeof (DurationPickerPluginConf).FullName);

    /// <summary>
    /// Used
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Not used
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// Set a positive number of milliseconds to activate their display
    /// </summary>
    public string Parameters
    {
      get
      {
        return TimeSpan.FromMilliseconds (this.WithMilliseconds ? 1 : 0).ToString ();
      }
      set
      {
        if (string.IsNullOrEmpty (value)) { // Default
          this.WithMilliseconds = false;
        }
        else {
          try {
            var timeSpan = TimeSpan.Parse (value);
            this.WithMilliseconds = timeSpan.Milliseconds != 0;
          }
          catch (Exception ex) {
            log.ErrorFormat ("Parameters.set: invalid value {0}", value);
            log.Error ("Parameters.set: exception", ex);
          }
        }
      }
    }

    /// <summary>
    /// Use milli-seconds (false by default)
    /// </summary>
    public bool WithMilliseconds { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public DurationPickerPluginConf ()
    {
      this.WithMilliseconds = false;
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    public virtual Action<IPluginConfig, string, string> AddControl
    {
      get
      {
        return AddDurationPickerControl;
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
    protected void AddDurationPickerControl (IPluginConfig pluginConfig, string name, string labelText)
    {
      if (this.Optional) {
        pluginConfig.AddOptionalDurationPicker (name, labelText, this.WithMilliseconds);
      }
      else {
        pluginConfig.AddDurationPicker (name, labelText, this.WithMilliseconds);
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
