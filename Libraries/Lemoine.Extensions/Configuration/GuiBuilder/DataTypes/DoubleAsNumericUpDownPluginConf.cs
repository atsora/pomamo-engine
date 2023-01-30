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
  /// Plugin conf to consider a NumericUpDown control is considered to set a double
  /// </summary>
  public class DoubleAsNumericUpDownPluginConf
    : NumericUpDownPluginConf
    , IPluginConfDataType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DoubleAsNumericUpDownPluginConf).FullName);

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return c => GetDoublePropertyToDecimal (property, c);
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Action<TConfiguration, object> GetConfigurationSetter<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return delegate (TConfiguration c, object v)
      { SetDoublePropertyFromDecimal (property, c, v); };
    }

    object GetDoublePropertyToDecimal<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      var o = GetProperty (propertyInfo, configuration);
      if (this.Optional) {
        var n = (double?)o;
        if (n.HasValue) {
          var d = n.Value;
          return (decimal?)(decimal)d;
        }
        else {
          return (decimal?)null;
        }
      }
      else {
        return (decimal)(double)o;
      }
    }

    void SetDoublePropertyFromDecimal<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      if (this.Optional) {
        decimal? n = (decimal?)v;
        double? d;
        if (n.HasValue) {
          d = (double?)n.Value;
        }
        else {
          d = (double?)null;
        }
        SetProperty (propertyInfo, configuration, d);
      }
      else {
        double d = (double)(decimal)v;
        SetProperty (propertyInfo, configuration, d);
      }
    }
  }
}
