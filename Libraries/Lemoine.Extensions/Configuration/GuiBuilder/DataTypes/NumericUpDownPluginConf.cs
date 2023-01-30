// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider a NumericUpDown control will be used to get a decimal
  /// </summary>
  public class NumericUpDownPluginConf : IPluginConfDataType
  {
    ILog log = LogManager.GetLogger (typeof (NumericUpDownPluginConf).FullName);

    /// <summary>
    /// Maximum
    /// </summary>
    public virtual decimal Maximum { get; protected set; }

    /// <summary>
    /// Decimal places
    /// </summary>
    public virtual int DecimalPlaces { get; protected set; }

    /// <summary>
    /// Not used
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Not used
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// Parameters:
    /// <item>xxx.xx0 as a maximum with the right number of decimal values</item>
    /// <item>maximum:decimalPlaces</item>
    /// 
    /// Default is 9999.99
    /// </summary>
    public virtual string Parameters {
      get
      {
        var maximum = this.Maximum.ToString (CultureInfo.InvariantCulture);
        var dotSplit = maximum.Split (new char[] { '.' }, 2, StringSplitOptions.None);
        string parameters = maximum;
        int existingDecimalDigits;
        if (1 == dotSplit.Length) {
          if (0 == this.DecimalPlaces) {
            return ((long)this.Maximum).ToString (CultureInfo.InvariantCulture);
          }
          parameters += ".";
          existingDecimalDigits = 0;
        }
        else {
          existingDecimalDigits = dotSplit[1].Length;
        }
        for (int i = existingDecimalDigits; i < this.DecimalPlaces; ++i) {
          parameters += "0";
        }
        return parameters;
      }
      set
      {
        try {
          var strings = value.Split (new char[] { ':' }, 2, StringSplitOptions.None);
          if (2 == strings.Length) {
            this.Maximum = decimal.Parse (strings[0]);
            this.DecimalPlaces = int.Parse (strings[1]);

          }
          else { // == 1
            var dotSplit = strings[0].Split (new char[] { '.' }, 2, StringSplitOptions.None);
            this.Maximum = decimal.Parse (dotSplit[0]);
            if (1 == dotSplit.Length) {
              this.DecimalPlaces = 0;
            }
            else {
              this.DecimalPlaces = dotSplit[1].Length;
            }
          }
        }
        catch (Exception ex) {
          log.Error ($"Parameters.set: exception on value={value} => fallback to 9999.99", ex);
          this.Maximum = 9999.99m;
          this.DecimalPlaces = 2;
        }
      }
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    public virtual Action<IPluginConfig, string, string> AddControl
    {
      get
      {
        return AddNumericUpDownControl;
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
    protected void AddNumericUpDownControl (IPluginConfig pluginConfig, string name, string labelText)
    {
      if (this.Optional) {
        pluginConfig.AddOptionalNumericUpDown (name, labelText, this.Maximum, this.DecimalPlaces);
      }
      else {
        pluginConfig.AddNumericUpDown (name, labelText, this.Maximum, this.DecimalPlaces);
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
