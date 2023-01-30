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
  /// Plugin conf to consider a NumericUpDown control is considered to set an integer
  /// </summary>
  public class IntAsNumericUpDownPluginConf
    : NumericUpDownPluginConf
    , IPluginConfDataType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DoubleAsNumericUpDownPluginConf).FullName);

    /// <summary>
    /// Decimal places
    /// </summary>
    public override int DecimalPlaces {
      get { return 0; }
    }

    /// <summary>
    /// Parameters: 10000
    /// </summary>
    public override string Parameters
    {
      get { return ((long)this.Maximum).ToString (); }
      set
      {
        base.Parameters = value;
        this.DecimalPlaces = 0;
      }
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return c => GetIntPropertyToDecimal (property, c);
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
      { SetIntPropertyFromDecimal (property, c, v); };
    }

    object GetIntPropertyToDecimal<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      var o = GetProperty (propertyInfo, configuration);
      if (this.Optional) {
        var n = (int?)o;
        if (n.HasValue) {
          var d = n.Value;
          return (decimal?)(decimal)d;
        }
        else {
          return (decimal?)null;
        }
      }
      else {
        return (decimal)(int)o;
      }
    }

    void SetIntPropertyFromDecimal<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      if (this.Optional) {
        decimal? n = (decimal?)v;
        int? d;
        if (n.HasValue) {
          d = (int?)n.Value;
        }
        else {
          d = (int?)null;
        }
        SetProperty (propertyInfo, configuration, d);
      }
      else {
        int d = (int)(decimal)v;
        SetProperty (propertyInfo, configuration, d);
      }
    }
  }
}
